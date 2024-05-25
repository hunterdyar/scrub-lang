﻿using System.Net;
using scrub_lang.Compiler;
using scrub_lang.Evaluator;
using scrub_lang.Objects;
using scrub_lang.Parser;
using Array = scrub_lang.Objects.Array;
using Object = scrub_lang.Objects.Object;
using String = scrub_lang.Objects.String;

namespace scrub_lang.VirtualMachine;

public class VM
{
	//references for passing around everything with just the vm object.
	//end keeping the last-used parser from getting gc'd.
	public static Parser.Parser Parser { get; private set; }
	public static Tokenizer.Tokenizer Tokenizer { get; private set; }
	//
	public const int StackSize = 2048;
	public const int GlobalsSize = UInt16.MaxValue;
	//some consts because why have many number when two number do.
	public static readonly Bool True = new Bool(true);
	public static readonly Bool False = new Bool(false);
	public static readonly Null Null = new Null();
	private Dictionary<OpCode, Stack<bool>> _conditionalHistory = new Dictionary<OpCode, Stack<bool>>(); 
	public ByteCode ByteCode { get; }

	public Stack<Frame> Frames = new Stack<Frame>();//todo: replace this with pre-allocated array (for SPEeeEEed)
	public Stack<Frame> Unframes = new Stack<Frame>();
	
	private int frameIndex;
	
	private List<Object> constants;
	private object[] stack;//I do think I want to replace this with my own base ScrubObject? Not sure.
	private object[] unstack;//I am extremely split on calling this the UnStack or the AntiStack.
	public Object[] Globals => globals;
	private Object[] globals;//globals store. 
	//StackPointer will always point to the next free slot in the stack. Top element will be sp-1
	//we put something in SP, then increment it.
	private int sp;//stack pointer
	private int usp; //unstack pointer.
	public TextWriter outputStream;
	
	//the ENGINE
	public VMState State => _state;
	private VMState _state;

	//just caches
	private int ip;
	private Frame _frame;
	//tdoo: refactor constructors
	public VM(ByteCode byteCode, TextWriter? writer = null)
	{
		//optional non-default output.
		outputStream = writer;
		if (outputStream == null)
		{
			outputStream = Console.Out;
		}

		_state = VMState.Initialized;
		ByteCode = byteCode;//keep a copy.
		_conditionalHistory.Add(OpCode.OpJumpNotTruthy, new Stack<bool>());

		//instructions
		var mainFunction = new Function(byteCode.Instructions, byteCode.NumSymbols,false);
		var mainClosure = new Closure(mainFunction);//all functions are closures. The program is a function. the program is a closure. it's closures all the way down.
		var mainFrame = new Frame(mainClosure,0,-1);
		Frames = new Stack<Frame>();
		Frames.Push(mainFrame);
		constants = byteCode.Constants.ToList();
		stack = new object[StackSize];//todo: will this become a different base type? 
		//todo: list for the unstack. or some kind of growable collection. 
		//it's going to need to be at least..... 2 times larger than this. (actually infinity overhead).
		//now, some things we pop off of the stack are knowable from state, because the function is deterministic.
		//most aren't cus that's why have a stack and not instructions.
		unstack = new object[StackSize];
		globals = new Object[GlobalsSize];
		sp = 0;
		usp = 0;
	}

	public VM(ByteCode byteCode, Object[] globalsStore, TextWriter writer = null)
	{
		outputStream = writer;
		if (outputStream == null)
		{
			outputStream = Console.Out;
		}

		_state = VMState.Initialized;
		ByteCode = byteCode; //keep a copy.
		_conditionalHistory.Add(OpCode.OpJumpNotTruthy,new Stack<bool>());
		//instructions
		var mainFunction = new Function(byteCode.Instructions, byteCode.NumSymbols, false);//we track numSymbols here just for fun. 
		var mainClosure = new Closure(mainFunction);
		var mainFrame = new Frame(mainClosure,0,-1);
		Frames = new Stack<Frame>();
		Frames.Push(mainFrame);

		constants = byteCode.Constants.ToList();
		stack = new object[StackSize]; //todo: will this become a different base type? 
		unstack = new object[StackSize];
		globals = globalsStore;
		sp = 0;
		usp = 0;
	}

	
	public ScrubVMError? Run()
	{
		ScrubVMError? res = null;
		if (_state == VMState.Initialized)
		{
			_state = VMState.Running;
		}
		else if(_state == VMState.Paused)
		{
			_state = VMState.Running;
		}
		else
		{
			Console.WriteLine("Can't run, already running!");
		}
		//is paused or initiated, jump on in!
		while (_state == VMState.Running)
		{
			res = RunOne();
			if (res != null)
			{
				_state = VMState.Error;
			}
		}

		return res;
	}

	public void Pause()
	{
		if (_state != VMState.Paused)
		{
			_state = VMState.Paused;
		}
	}
	
	public ScrubVMError? RunOne()
	{
		if (CurrentFrame().ip >= CurrentFrame().Instructions().Length - 1)
		{
			_state = VMState.Complete;
			return null;
		}

		//this is the hot path. We actually care about performance.
		byte[] ins;
		ScrubVMError? error = null;
		//ip is instructionPointer
	
		CurrentFrame().ip++;//increment at start instead of at end because of all the returns. THis is why we init the frame with an ip of -1.
		//fetch -> decode -> execute
		ip = CurrentFrame().ip;
		ins = CurrentFrame().Instructions();
		//fetch
		OpCode op = (OpCode)ins[ip];
		//decode
		switch (op)
		{
			case OpCode.OpConstant:
				var constIndex = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]); 
				CurrentFrame().ip += 3; //increase the number of bytes re read to decode the operands. THis leaves the next instruction pointing at an OpCode.
				return Push(constants[constIndex]);
				break;
			case OpCode.OpCall:
				var numArgs = Op.ReadUInt8(ins[ip + 1]);
				CurrentFrame().ip += 2; //move instruction pointer past the operand.
				return ExecuteFunction(numArgs);
				break;
			case OpCode.OpClosure:
				var cIndex = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]);
				var numFreeVars = Op.ReadUInt8(ins[ip + 3]);
				CurrentFrame().ip += 4;
				return PushClosure((int)cIndex, (int)numFreeVars);
			case OpCode.OpReturnValue:
				var returnValue = Pop();
				_frame = PopFrame();
				//pop. The -1 gets rid of the function call too.
				//after storing the return value top of stack), remove all locals and free's:
				while (sp > _frame.basePointer - 1)
				{
					Pop();
				}
				
				//put the return value back on top of the stack.
				return Push(returnValue);//stack the result of 
			case OpCode.OpAdd:
			case OpCode.OpSubtract:
			case OpCode.OpMult:
			case OpCode.OpDivide:
			case OpCode.OpBitAnd:
			case OpCode.OpBitOr:
			case OpCode.OpBitXor:
				return RunBinaryOperation(op);
			case OpCode.OpBitShiftLeft:
			case OpCode.OpBitShiftRight:
				return RunBitShiftOperation(op);
			case OpCode.OpConcat:
				return new ScrubVMError("Concatenation Operator Not Yet Implemented");
			case OpCode.OpPop:
				Pop();
				return null;
			case OpCode.OpTrue:
				return Push(True);
			case OpCode.OpFalse:
				return Push(False);
			case OpCode.OpEqual:
			case OpCode.OpNotEqual:
			case OpCode.OpGreaterThan:
				return RunComparisonOperation(op);
			case OpCode.OpBang:
				return RunBangOperator(op);
			case OpCode.OpNegate:
				return RunNegateOperator(op);
			case OpCode.OpJump:
				int pos = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]);
				CurrentFrame().ip = pos - 1; //+1 when the loop ends :p
				return null;
			case OpCode.OpJumpNotTruthy:
				pos = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]);
				CurrentFrame().ip += 2; //skipPastTheJump if we are truthy
				var condition = PopScrubObject();
				bool jmp = !IsTruthy(condition);
				_conditionalHistory[OpCode.OpJumpNotTruthy].Push(jmp);
				if (jmp)
				{
					CurrentFrame().ip = pos - 1;
				}
				return null;
			case OpCode.OpNull:
				return Push(Null);
			case OpCode.OpSetGlobal:
				var globalIndex = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]);
				CurrentFrame().ip += 3;//plus sandwich
				globals[globalIndex] = PopScrubObject();
				return null;
			case OpCode.OpSetLocal:
				var localIndex = Op.ReadUInt8(ins[ip + 1]);
				CurrentFrame().ip += 2;
				_frame = CurrentFrame();
				//set the stack in our buffer area to our object. THis is going to be a tricky one to UNDO
				stack[_frame.basePointer + (int)localIndex] = PopScrubObject(); //popscrubObject is to force errors if we pop an instruction.
				return null;
			case OpCode.OpGetGlobal:
				globalIndex = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]);
				CurrentFrame().ip += 3;
				Push(globals[globalIndex]);
				return null;
			case OpCode.OpGetLocal:
				localIndex = Op.ReadUInt8(ins[ip + 1]);
				CurrentFrame().ip += 2;
				_frame = CurrentFrame();
				return Push(stack[_frame.basePointer + (int)localIndex]);
			case OpCode.OpGetBuiltin:
				var builtInIndex = Op.ReadUInt8(ins[ip + 1]);
				CurrentFrame().ip += 2;
				var def = Builtins.AllBuiltins[builtInIndex];
				return Push(def.Builtin);
			case OpCode.OpGetFree:
				var freeIndex = Op.ReadUInt8(ins[ip + 1]);
				CurrentFrame().ip += 2;
				var currentClosure = CurrentFrame().closure;
				return Push(currentClosure.FreeVariables[freeIndex]);
			case OpCode.OpCurrentClosure:
				currentClosure = CurrentFrame().closure;
				return Push(currentClosure);
			case OpCode.OpArray:
				int numElements = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]);
				CurrentFrame().ip += 3;
				var array = BuildArray(sp - numElements, sp);
				sp = sp - numElements;
				return Push(array);
			case OpCode.OpIndex:
				var index = PopScrubObject();
				var left = PopScrubObject();
				return RunIndexExpression(left, index);
		}
		return null;
		
	}
	public ScrubVMError? PreviousOne()
	{
		if (CurrentFrame().ip <= 0)
		{
			_state = VMState.Paused;
			Console.WriteLine("At beginning, can't undo!");
			return null;
		}

		//this is the hot path. We actually care about performance.
		byte[] ins;
		ScrubVMError? error = null;
		//ip is instructionPointer
	
		// CurrentFrame().ip;
		ip = CurrentFrame().ip;
		ins = CurrentFrame().Instructions();
		//fetch
		OpCode op = (OpCode)ins[ip];
		//decode
		switch (op)
		{
			case OpCode.OpConstant:
				UnPush();
				var constIndex = Op.ReadUInt16([ins[ip -2], ins[ip -1]]); 
				CurrentFrame().ip -= 4; //increase the number of bytes re read to decode the operands. THis leaves the next instruction pointing at an OpCode.
				return null;
			case OpCode.OpCall:
				var p = UnPush();//first, trash the result of the return.
				var numArgs = Op.ReadUInt8(ins[ip -1 ]);
				CurrentFrame().ip -= 3; //move instruction pointer past the operand + sandwich
				return DeexecuteFunction(numArgs);
				break;
			case OpCode.OpClosure:
				var cIndex = Op.ReadUInt16([ins[ip - 3], ins[ip - 2]]);
				var numFreeVars = Op.ReadUInt8(ins[ip - 1]);
				CurrentFrame().ip -= 5;//op, index, index, vars, op
				UnPush();
				return null;
				//return PushClosure((int)cIndex, (int)numFreeVars);
			case OpCode.OpReturnValue:
				//we jump past the return value, and already popped it off the stack in anti-call.
				_frame = PopFrame();
				//pop. The -1 gets rid of the function call too.
				//sp = _frame.basePointer - 1;
				return null;
			case OpCode.OpAdd:
			case OpCode.OpSubtract:
			case OpCode.OpMult:
			case OpCode.OpDivide:
			case OpCode.OpBitAnd:
			case OpCode.OpBitOr:
			case OpCode.OpBitXor:
			case OpCode.OpBitShiftLeft:
			case OpCode.OpBitShiftRight:
				//binary ops are all the same. pop pop push. we reverse it with poppushpush.
				UnPush();//remove the result of the op
				UnPop();//restore the previous values. I think this will do it in the right order?
				UnPop();
				CurrentFrame().ip--;
				return null;
			case OpCode.OpConcat:
				return new ScrubVMError("Concatenation Operator Not Yet Implemented");
			case OpCode.OpPop:
				UnPop();
				CurrentFrame().ip--;
				return null;
			case OpCode.OpTrue:
				var o = UnPush();
				if ((Bool)o != True)
				{
					throw new VMException($"Reverse error (expected True, got {o}");
				}

				CurrentFrame().ip--;
				return null;
			case OpCode.OpFalse:
				UnPush();
				CurrentFrame().ip--;
				return null;
			case OpCode.OpEqual:
			case OpCode.OpNotEqual:
			case OpCode.OpGreaterThan:
				UnPush(); //remove the result of the op
				UnPop(); //restore the previous values
				UnPop();
				CurrentFrame().ip--;
				return null;
			case OpCode.OpBang:
			case OpCode.OpNegate:
				//todo: can't we just run the negate operator again?
				UnPush();
				UnPop();
				CurrentFrame().ip--;
				return null;
			case OpCode.OpJump:
				int pos = Op.ReadUInt16([ins[ip - 2], ins[ip - 1]]);
				CurrentFrame().ip = pos - 1; //+1 when the loop ends :p
				return null;
			case OpCode.OpJumpNotTruthy:
				//push truthy value w checked before back onto the stack.
				pos = Op.ReadUInt16([ins[ip - 2], ins[ip - 1]]);
				CurrentFrame().ip -= 2;
				var condition = UnPop();
				bool jmp = !IsTruthy((Object)condition);
				_conditionalHistory[OpCode.OpJumpNotTruthy].Push(jmp);
				if (jmp)
				{
					CurrentFrame().ip = pos - 1;//pos-1 or pos+1? or pos? can't think, trial-and-error-ing.
				}
				return null;
			case OpCode.OpNull:
				o = UnPush();
				if ((Null)o != Null)
				{
					throw new VMException($"Reverse error (expected True, got {o}");
				}

				CurrentFrame().ip--;
				return null;
			case OpCode.OpSetGlobal:
				o = UnPop();
				var globalIndex = Op.ReadUInt16([ins[ip - 2], ins[ip -1]]);
				CurrentFrame().ip -= 4;//plus sandwich
				globals[globalIndex] = (Object)o;
				return null;
			case OpCode.OpSetLocal:
				o = UnPop();//todo: this won't work, the previous value MIGHT be garbage!
				var localIndex = Op.ReadUInt8(ins[ip - 1]);
				CurrentFrame().ip -= 3;
				_frame = CurrentFrame();
				//set the stack in our buffer area to our object. THis is going to be a tricky one to UNDO
				if (o == null)
				{
					//when we set it the first time, it was WHO KNOWs. now? who knows.
					stack[_frame.basePointer + (int)localIndex] = Null; //popscrubObject is to force errors if we pop an instruction.
				}
				else
				{
					stack[_frame.basePointer + (int)localIndex] = o; //popscrubObject is to force errors if we pop an instruction.
				}
				return null;
			case OpCode.OpGetGlobal:
				globalIndex = Op.ReadUInt16([ins[ip -2], ins[ip + 1]]);
				CurrentFrame().ip -= 4;
				// Push(globals[globalIndex]);
				UnPush();
				return null;
			case OpCode.OpGetLocal:
				localIndex = Op.ReadUInt8(ins[ip - 1]);
				CurrentFrame().ip -= 3;
				//_frame = CurrentFrame();
				UnPush();
				// return Push(stack[_frame.basePointer + (int)localIndex]);
				return null;
			case OpCode.OpGetBuiltin:
				var builtInIndex = Op.ReadUInt8(ins[ip - 1]);
				CurrentFrame().ip -= 3;
				var def = Builtins.AllBuiltins[builtInIndex];
				UnPush();
				return null;
				//return Push(def.Builtin);
			case OpCode.OpGetFree:
				var freeIndex = Op.ReadUInt8(ins[ip - 1]);
				CurrentFrame().ip -= 3;
				var currentClosure = CurrentFrame().closure;
				//return Push(currentClosure.FreeVariables[freeIndex]);
				UnPush();
				return null;
			case OpCode.OpCurrentClosure:
				currentClosure = CurrentFrame().closure;
				// return Push(currentClosure);
				UnPush();
				CurrentFrame().ip--;
				return null;
			case OpCode.OpArray:
				int numElements = Op.ReadUInt16([ins[ip - 2], ins[ip -1]]);
				var array = BuildArray(sp - numElements, sp);
				sp = sp - numElements;
				UnPush();
				CurrentFrame().ip -= 4;
				return null;
				// return Push(array);
			case OpCode.OpIndex:
				UnPush();
				var left = UnPop();
				var index = UnPop();
				// return RunIndexExpression(left, index);
				CurrentFrame().ip --;
				return null;
		}
		return null;
	}

	private ScrubVMError? DeexecuteFunction(int numArgs)
	{
		//so... we need to go up a frame, into the function again, undo it, then come out the other side.
		//a call increased our IP. undoing it should.. still do that (before this part)
		//this isn't the state of the stack......
		var callee = UnPop();//get the closure.
		//var callee = stack[sp - 2 - numArgs];//we are at the same location as before in the stack... no we aren't, there's also a return value.
		switch (((Object)callee).GetType())
		{
			case ScrubType.Closure:
				return UnCallClosure((Closure)callee, numArgs);
			case ScrubType.Builtin:
				//uhhhh
				UnPop();
				return null;
				break;
			default:
				break;
		}

		return new ScrubVMError($"Unable to UnExecute Function. Obj \"{callee}\" is not a function.");

	}
	private ScrubVMError? ExecuteFunction(int numArgs)
	{
		//reach further down the stack to get the function, past the locals (arguments)
		var callee = stack[sp - 1 - numArgs];
		switch (((Object)callee).GetType())
		{
			case ScrubType.Closure:
				return CallClosure((Closure)callee, numArgs);
			case ScrubType.Builtin:
				return CallBuiltin((Builtin)callee, numArgs);
			case ScrubType.Function:
				return new ScrubVMError("Trying to Execute Function that isn't wrapped as a closure!");
		}

		return new ScrubVMError($"Unable to Execute Function. Obj \"{callee}\" is not a function.");
	}

	private ScrubVMError? CallBuiltin(Builtin fn, int numArgs)
	{
		var args = new Object[numArgs];
		//a lazy slice copy. fast tho.
		for (int i = 0; i < numArgs; i++)
		{
			args[i] = (Object)stack[sp - numArgs + i];
		}
		//the actual funtion call
		var result = fn.Function(this,args);
		//it is the VM's duty to pop the function calls off.
		sp = sp - numArgs - 1;
		if (result != null)
		{
			Push(result);
		}
		else
		{
			Push(Null);
		}

		return null;
	}

	private ScrubVMError? CallClosure(Closure cl, int numArgs)
	{
		if (numArgs != cl.CompiledFunction.NumLocals)
		{
			//todo: write tests for this.
			return new ScrubVMError("Wrong number of arguments!");
		}
		var frame = new Frame(cl,sp-numArgs);
		PushFrame(frame);
		sp = frame.basePointer + cl.CompiledFunction.NumLocals;//give us a buffer of the number of local variables we will store in this area on the stack.
		return null;
	}

	private ScrubVMError? UnCallClosure(Closure cl, int numArgs)
	{
		if (numArgs != cl.CompiledFunction.NumLocals)
		{
			return new ScrubVMError("Wrong number of arguments? huh?");
		}
		//go into a frame like normal, then move up the stack and call?
		//when we reach the top of the function, we will need to take it off the stack, but the call is a lateral move to a new Instructions, basically.
		var frame = Unframes.Pop();
		
		
		PushFrame(frame);
		//sp = frame.basePointer + cl.CompiledFunction.NumLocals;
		while (sp < _frame.basePointer +cl.CompiledFunction.NumLocals)
		{
			UnPop();
		}
		return null;
	}

	private ScrubVMError? RunIndexExpression(Object left, Object index)
	{
		var lt = left.GetType();
		var it = index.GetType();
		if (lt == ScrubType.Array && it == ScrubType.Int)
		{
			//execute array index
			return RunArrayIndex((Array)left, (Integer)index);
		}else if (lt == ScrubType.Int && it == ScrubType.Int)
		{
			//This could actually work on .... all the types? It would just be terrible. We'd have to manually check the length of the byte data.
			
			var i = ((Integer)index).NativeInt;
			//Get the bit (as a bool?) at the binary position in the array.
			//They are stored as 32 bit signed integers, so thats' 4 bytes.
			if (i < 0)
			{
				return null;
			}else if(i<left.Bits.Count){
				return Push(left.Bits.Get(i) ? True : False);//not forgetting to go from internal bool to scrub bool.
			}
			else
			{
				return Push(Null);
			}
		}else if (lt != ScrubType.Null && it == ScrubType.Int) {
			//get the byte data. booleans return 1/0, strings return uni8 characters, and functions return their bytecode.
			//it's read-only right now, however.
			var i = ((Integer)index).NativeInt;
			var bytes = left.AsByteArray();
			var max = bytes.Length - 1;
			if (i < 0 || i > max)
			{
				return Push(Null);
			}
			//todo: we don't have a byte type (yet, it's there but untested/unexpected)... or a char type. Keep your UTF8 lookup tables handly!
			return Push(new Integer(bytes[i]));
		}else{
			return new ScrubVMError($"Unable to index {lt} with {it}");
		}

		return null;
	}

	private ScrubVMError? RunArrayIndex(Array array, Integer index)
	{
		var i = index.NativeInt;
		var max = array.Length.NativeInt-1;
		if (i < 0 || i > max)
		{
			return Push(Null);
		}
		return Push(array.NativeArray[i]);
	}

	public Object BuildArray(int startIndex, int endIndex)
	{
		var elements = new Object[endIndex - startIndex];
		for (int i = startIndex; i < endIndex; i++)
		{
			elements[i - startIndex] = (Object)stack[i];//todo: wrap this cast with error handling.
		}

		return new Array(elements);
	}

	private ScrubVMError? RunNegateOperator(OpCode op)
	{
		var operand = PopScrubObject();
		if (operand.GetType() == ScrubType.Int)
		{
			return Push(new Integer(-(operand as Integer).NativeInt));
		}

		return new ScrubVMError($"Unable to negate (-) {operand}");
	}

	private ScrubVMError? RunBangOperator(OpCode op)
	{
		var operand = PopScrubObject();

		if (operand == True)
		{
			return Push(False);
		}else if (operand == False)
		{
			return Push(True);
		}else if (operand == Null)
		{
			return Push(True);
		}
		else
		{
			//uh oh. warning? everythnig that is not false is truthy. Not so sure! todo: Investigate truthiness table.
			return Push(False);
		}
	}

	private ScrubVMError? RunComparisonOperation(OpCode op)
	{
		var r = PopScrubObject();
		var l = PopScrubObject();

		if (l.GetType() == ScrubType.Int && r.GetType() == ScrubType.Int)
		{
			return RunIntegerComparisonOperation(op, (Integer)l, (Integer)r);
		}
		switch (op)
		{
			case OpCode.OpEqual:
				return Push(NativeBoolToBooleanObject(l == r));
			case OpCode.OpNotEqual:
				return Push(NativeBoolToBooleanObject(l != r));
			default:
				return new ScrubVMError($"Unknown Operator {{op}} for {l.GetType()} and {r.GetType()}");
		}
	}

	private ScrubVMError? RunIntegerComparisonOperation(OpCode op, Integer a, Integer b)
	{
		switch (op)
		{
			//comparing native types here means we can't do false == 0, which scrub should evaluate to true.
			case OpCode.OpEqual:
				return Push(NativeBoolToBooleanObject(a.NativeInt == b.NativeInt));
			case OpCode.OpNotEqual:
				return Push(NativeBoolToBooleanObject(a.NativeInt != b.NativeInt));
			case OpCode.OpGreaterThan:
				return Push(NativeBoolToBooleanObject(a.NativeInt > b.NativeInt));
			default:
				return new ScrubVMError($"Unknown Operator {op}");
		}
	}

	private ScrubVMError? RunBitShiftOperation(OpCode op)
	{
		//todo: these are broken because, i think, bitwise things?
		var r = PopScrubObject();
		var l = PopScrubObject();
		var rightType = r.GetType();
		
		if (op == OpCode.OpBitShiftLeft && rightType == ScrubType.Int)
		{
			l.BitShiftLeft(((Integer)r).NativeInt);
			return Push(l);
		}
		else if (op == OpCode.OpBitShiftRight && rightType == ScrubType.Int)
		{
			l.BitShiftLeft(((Integer)r).NativeInt);
			return Push(l);
		}
		else
		{
			//if opcode is not bitshift, which feel sunlikely enough to ignore it for now.
			if (op != OpCode.OpBitShiftLeft && op != OpCode.OpBitShiftRight)
			{
				return new ScrubVMError("Trying to bit shift, but wrong operator? ");
			}
			else if(rightType != ScrubType.Int)
			{
				return new ScrubVMError("Cannot Bitshift by non-integer amount");
			}
		}

		return null;
	}

	private ScrubVMError? RunBinaryOperation(OpCode op)
	{
		var r = PopScrubObject();
		var l = PopScrubObject();
		
		//Do we cast to Object here, to get the type?
		var leftType = l.GetType();
		var rightType = r.GetType();

		
		//hmmm. Feeels like we are casting twice. But, hey, at least we are sure it will work.
		//I low-key want to use a single type for all of the underlying objects, and just switch case? is that stupid? It feels stupid, but casting everywhere does too. 
		//I thought aobut it and yeah its stupid. Just weird to write a dynamic-ish language in a staticly typed one no matter which way you slice it.
		if(leftType == ScrubType.Int && rightType == ScrubType.Int)
		{
			return RunBinaryIntegerOperation(op,  (Integer)l, (Integer)r);
		}else if (leftType == ScrubType.String && rightType == ScrubType.String)
		{
			return RunBinaryStringOperation(op, (String)l, (String)r);
		}else if (leftType == ScrubType.Bool && rightType == ScrubType.Bool)
		{
			return RunBinaryBooleanOperation(op, (Bool)l, (Bool)r);
		}

		//we do this after the direct checks, because I think it will be faster this way.
		//It handles all of the other objects/pairings, using strings made from nativeToString.
		if (op == OpCode.OpConcat)
		{
			return RunConcatOperation(op, l, r);
		}
		
		return new ScrubVMError($"Unsupported types for operation {op}");
	}

	private ScrubVMError? RunBinaryBooleanOperation(OpCode op, Bool left, Bool right)
	{
		Bool result = new Bool(false);
		switch (op)
		{
			//todo: move these to overrides of the operators declared in the object class (see int).
			case OpCode.OpBitAnd:
				result = new Bool(left.NativeBool & right.NativeBool);
				break;
			case OpCode.OpBitOr:
				result = new Bool(left.NativeBool | right.NativeBool);
				break;
			case OpCode.OpBitXor:
				result = new Bool(left.NativeBool ^ right.NativeBool);
				break;
			default:
				return new ScrubVMError($"Unsupported binary operation {op} for 2 booleans.");
				break;
		}

		return Push(result);
	}

	private ScrubVMError? RunBinaryIntegerOperation(OpCode op, Integer left, Integer right)
	{
		Integer result;
		switch (op)
		{
			case OpCode.OpAdd: 
				result = left + right;
				break;
			case OpCode.OpSubtract:
				result = left - right;
				break;
			case OpCode.OpMult:
				result = left * right;
				break;
			case OpCode.OpDivide:
				result = left / right;
				break;
			case OpCode.OpBitAnd:
				result = left & right;
				break;
			case OpCode.OpBitOr:
				result = left | right;
				break;
			case OpCode.OpBitXor:
				result = left ^ right;
				break;
			case OpCode.OpConcat:
				//todo: optimize these to use internal casts, instead of round-trip through native casts.
				//todo: should 1++2 return 12 or "12". right now it's "12", which i think is right.
				return RunBinaryStringOperation(op, new String(left.ToString()), new String(right.ToString()));
			default:
				return new ScrubVMError($"Unkown Integer Operation {op}");
		}
		return Push(result);
	}

	private ScrubVMError? RunBinaryStringOperation(OpCode op, String left, String right)
	{
		String result = new String("");
		switch (op)
		{
			case OpCode.OpAdd:
				result = new String(left,right);
				break;
			case OpCode.OpConcat:
				result = new String(left, right);
				break;
			default:
				return new ScrubVMError($"Unkown Integer Operation {op}");
		}

		return Push(result);
	}

	private ScrubVMError? RunConcatOperation(OpCode op, Object left, Object right)
	{
		String ls;
		if (left.GetType() != ScrubType.String)
		{
			ls = left.ToScrubString();
		}
		else
		{
			ls = (String)(left);
		}

		String rs;
		if (right.GetType() != ScrubType.String)
		{
			rs = right.ToScrubString();
		}
		else
		{
			rs = (String)right;
		}

		return RunBinaryStringOperation(op,ls, rs);
	}
	private ScrubVMError? Push(object o)
	{
		if (sp >= StackSize)
		{
			return new ScrubVMError("Stack Overflow!");
		}
		
		stack[sp] = o;
		sp++;
		return null;
	}

	private object UnPop()
	{
		if (sp >= StackSize)
		{
			return new ScrubVMError("Stack Overflow!");
		}

		var o = unstack[usp-1];
		usp--;
		stack[sp] = o;
		sp++;
		return o;
	}

	private ScrubVMError? PushClosure(int closureConstIndex, int numFree)
	{
		var constant = constants[closureConstIndex];//todo: type check throw error.
		var fn = (Function)constant;

		//pull the free objects off of the stack and shove them into the closure object at runtime.
		//the order is important.
		var free = new Object[numFree];
		for (var i = 0; i < free.Length; i++)
		{
			free[i] = (Object)stack[sp - numFree + i];
		}

		sp = sp - numFree;

		var closure = new Closure(fn,free);//runtime closure from function! That's the whole point of closures!
		Push(closure);
		return null;
	}

	private object Pop()
	{
		var o = stack[sp - 1];
		sp--;
		unstack[usp] = o;
		usp++;
		return o;
	}

	private object UnPush()
	{
		var o = stack[sp - 1];
		sp--;
		return o;
	}

	public Object PopScrubObject()
	{
		//basically calling pop, but faster to just copy/paste. (i know!? wild).
		var o = stack[sp - 1];
		sp--;
		unstack[usp] = o;
		usp++;
		if (!(o is Object so))
		{
			throw new VMException($"Unable To Pop ScrubObject. Popped {o} instead");
		}

		return so;
	}
	public object LastPopped()
	{
		return stack[sp];
	}

	public Frame CurrentFrame() => Frames.Peek();
	public void PushFrame(Frame f) { Frames.Push(f); }

	public Frame PopFrame()
	{
		var f = Frames.Pop();
		Unframes.Push(f);
		return f;
	}

	public static bool IsTruthy(Object obj)
	{
		switch (obj.GetType())
		{
			case ScrubType.Bool:
				return ((obj as Bool)!).NativeBool;
			case ScrubType.Int:
				//todo: is 0 falsy? this isn't the only place in code we assert truthyness. At least also the ! op.
			case ScrubType.Null:
				return false;
			default:
				return true;
		}
	}
	public static Bool NativeBoolToBooleanObject(bool b)
	{
		return b ? True : False;
	}
	public static IExpression Parse(string input)
	{
		Tokenizer = new Tokenizer.Tokenizer(input);
		Parser = new Parser.Parser(Tokenizer);
		return Parser.ParseProgram();
	}
}