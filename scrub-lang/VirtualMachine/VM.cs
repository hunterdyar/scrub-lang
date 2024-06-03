using scrub_lang.Compiler;
using scrub_lang.Evaluator;
using scrub_lang.Objects;
using Array = scrub_lang.Objects.Array;
using Object = scrub_lang.Objects.Object;
using String = scrub_lang.Objects.String;

namespace scrub_lang.VirtualMachine;

public class VM
{
	public const int StackSize = 2048;
	public const int GlobalsSize = UInt16.MaxValue;
	//some consts because why have many number when two number do.
	public static readonly Bool True = new Bool(true);
	public static readonly Bool False = new Bool(false);
	public static readonly Null Null = new Null();
	private Dictionary<OpCode, Stack<bool>> _conditionalHistory = new Dictionary<OpCode, Stack<bool>>();
	public Program Program;

	//todo: optimize staacks.
	public Stack<Frame> Frames = new Stack<Frame>();
	public Stack<Frame> Unframes = new Stack<Frame>();

	private List<Object> Constants => _constants;
	private List<Object> _constants;
	public Object[] Stack => _stack;
	private Object[] _stack = new Object[StackSize];//I do think I want to replace this with my own base ScrubObject? Not sure.
	private Stack<Object> _unstack = new Stack<Object>();//I am extremely split on calling this the UnStack or the AntiStack.
	public Object[] Globals => _globals;
	private Object[] _globals = new Object[GlobalsSize];//globals store. 
	//StackPointer will always point to the next free slot in the stack. Top element will be sp-1
	//we put something in SP, then increment it.
	public int StackPointer => sp;
	// ReSharper disable once InconsistentNaming
	private int sp = 0;//stack pointer
	// ReSharper disable once InconsistentNaming
	private int usp = 0; //unstack pointer.
	public Progress Progress = new Progress();

	public ExecutionLog.ExecutionLog Log = new ExecutionLog.ExecutionLog();
	//set a "max" opcode? or set one on 'complete'?
	public TextWriter OutputStream;
	
	//the ENGINE
	public VMState State => _state;
	private VMState _state = VMState.Initialized;

	//just caches
	// ReSharper disable once InconsistentNaming
	private int ip;
	private Frame _frame;
	
	public readonly SymbolTable Symbols;
	//tdoo: refactor constructors
	public VM(Program program, TextWriter? writer = null)
	{
		//optional non-default output.
		OutputStream = writer;
		if (OutputStream == null)
		{
			OutputStream = Console.Out;
		}
		Program = program;//keep a copy.
		
		//prime our conditional histories. Might not be using this anymore? i forget
		_conditionalHistory.Add(OpCode.OpJumpNotTruthy, new Stack<bool>());

		//reference for reports
		Symbols = program.Symbols;
		//instructions
		var mainFunction = new Function(program.Instructions, 0,program.Symbols,program.Lookup,false);
		var mainClosure = new Closure(mainFunction);//all functions are closures. The program is a function. the program is a closure. it's closures all the way down.
		var mainFrame = new Frame(mainClosure,0,-1);
		Frames.Push(mainFrame);
		_constants = program.Constants.ToList();
	}

	public void SetGlobals( Object[]? globalsStore)
	{
		if (globalsStore != null)
		{
			_globals = globalsStore;
		}
		else
		{
			//oops, we put a null in here.
		}
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
			if (_state == VMState.Running)
			{
				return new ScrubVMError("Can't Run VM that is already running");
			}
			else if (_state == VMState.Complete)
			{
				return new ScrubVMError("Nothing to run, execution complete.");
			}
			{
				//huh
				return null;
			}
		}
		//is paused or initiated, jump on in!
		while (_state == VMState.Running)
		{
			res = RunOne();
			if (res != null)
			{
				_state = VMState.Error;
				return res;
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
		else
		{
			Console.WriteLine("Notice: Pausing already paused VM.");
		}
	}

	public ScrubVMError? RunTo(long opNum)
	{
		if (Log.LogPointer < opNum)
		{ 
			_state = VMState.Paused;
			while ((_state == VMState.Running || _state == VMState.Paused) && Log.LogPointer < opNum)
			{
				var res = RunOne();
				if (res != null)
				{
					_state = VMState.Error;
					return res;
				}
			}

			if (_state != VMState.Complete)
			{
				_state = VMState.Paused;
			}
		}
		//log or progress?
		else if (Log.LogPointer > opNum)
		{
			
			_state = VMState.Paused;
			while ((_state == VMState.Running || _state == VMState.Paused) && Log.LogPointer > opNum)
			{
				var res = PreviousOne();
				if (res != null)
				{
					_state = VMState.Error;
					return res;
				}
			}

			if (_state != VMState.Initialized)
			{
				_state = VMState.Paused;
			}
		}

		return null;
	}
	public ScrubVMError? RunOne()
	{
		if (CurrentFrame.ip >= CurrentFrame.Instructions().Length - 1)
		{
			if (sp == 1)
			{
				return new ScrubVMError("We did not manage to pop the last object! we uh. we should do that.");
			}else if (sp > 1)
			{
				return new ScrubVMError("What's all this junk on the stack?");
			}

			Progress.SetCompleteToCurrent();
			_state = VMState.Complete;
			return null;
		}

		//this is the hot path. We actually care about performance.
		int[] ins;
		ScrubVMError? error = null;
		//ip is instructionPointer
	
		CurrentFrame.ip++;//increment at start instead of at end because of all the returns. THis is why we init the frame with an ip of -1.
		Progress.IncrementCount();
		//fetch -> decode -> execute
		ip = CurrentFrame.ip;
		ins = CurrentFrame.Instructions();
		Console.WriteLine($"({CurrentLocation}) Do: {Op.InstructionToString(ins[ip])}");

		var insBytes = BitConverter.GetBytes(ins[ip]);
		//fetch
		OpCode op = (OpCode)insBytes[0];
		//decode
		switch (op)
		{
			case OpCode.OpConstant:
				var constIndex = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				return Push(_constants[constIndex]);
			case OpCode.OpCall:
				var numArgs = Op.ReadUInt8(insBytes[1]);
				return ExecuteFunction(numArgs);
			case OpCode.OpClosure:
				var cIndex = Op.ReadUInt16([insBytes[1], insBytes[2]]); 
				var numFreeVars = Op.ReadUInt8(insBytes[3]);
				Log.AddOperation(Frames.Count,"Load Closure to stack",cIndex.ToString(),"");
				return PushClosure((int)cIndex, (int)numFreeVars);
			case OpCode.OpReturnValue:
				bool frwd = 0 == Op.ReadUInt8(insBytes[1]); //is this a return from function or a finished-undoing-function (fstart)
				if (!frwd) return null;
				var returnValue = Pop();
				_frame = PopFrame();
				//after storing the return value top of stack), remove all locals and free's:
				while (sp > _frame.basePointer-1) //sp = basepointer -1
				{
					Pop();
				}
				//put the return value back on top of the stack.
				return Push(returnValue);
			case OpCode.OpAdd:
			case OpCode.OpSubtract:
			case OpCode.OpMult:
			case OpCode.OpDivide:
			case OpCode.OpBitAnd:
			case OpCode.OpBitOr:
			case OpCode.OpBitXor:
			case OpCode.OpPow:
			case OpCode.OpMod:
			case OpCode.OpConcat:
				return RunBinaryOperation(op);
			case OpCode.OpBitShiftLeft:
			case OpCode.OpBitShiftRight:
				return RunBitShiftOperation(op);
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
				int pos = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				frwd = 0 == Op.ReadUInt8(insBytes[3]);//is this a jump or an un-jump?if
				if (!frwd)
				{
					return null;
				}
				CurrentFrame.ip = pos - 1; //+1 when the loop ends :p
				return null;
			case OpCode.OpJumpNotTruthy:
				pos = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				frwd = 0 == Op.ReadUInt8(insBytes[3]); //is this a jump or an un-jump?if
				if (!frwd)
				{
					return null;
				}
				var condition = Pop();
				bool jmp = !IsTruthy(condition);
				_conditionalHistory[OpCode.OpJumpNotTruthy].Push(jmp);
				if (jmp)
				{
					CurrentFrame.ip = pos - 1;
				}
				return null;
			case OpCode.OpNull:
				return Push(Null);
			case OpCode.OpSetGlobal:
				var globalIndex = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				//don't pop it, just assign it.
				_unstack.Push(_globals[globalIndex]);//save the previous value.
				_globals[globalIndex] = (Object)_stack[sp - 1];//was =PopScrobject. might need to be a pop-push to get undo's to work correctly.
				Log.AddOperation(Frames.Count, $"Assign Global",globalIndex.ToString(),_globals[globalIndex]?.ToString());
				return null;
			case OpCode.OpSetLocal:
				var localIndex = Op.ReadUInt8(insBytes[1]);
				_frame = CurrentFrame;
				_unstack.Push(_stack[_frame.basePointer + (int)localIndex]);//save old value to history.
				//set the stack in our buffer area to our object. THis is going to be a tricky one to UNDO
				_stack[_frame.basePointer + (int)localIndex] = (Object)_stack[sp - 1];
				Log.AddOperation(Frames.Count, $"Assign Local", localIndex.ToString(), _globals[localIndex]?.ToString());
				return null;
			case OpCode.OpGetGlobal:
				globalIndex = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				Push(_globals[globalIndex]);
				return null;
			case OpCode.OpGetLocal:
				localIndex = Op.ReadUInt8(insBytes[1]);
				_frame = CurrentFrame;
				return Push(_stack[_frame.basePointer + (int)localIndex]);
			case OpCode.OpGetBuiltin:
				var builtInIndex = Op.ReadUInt8(insBytes[1]);
				var def = Builtins.AllBuiltins[builtInIndex];
				return Push(def.Builtin);
			case OpCode.OpGetFree:
				var freeIndex = Op.ReadUInt8(insBytes[1]);
				var currentClosure = CurrentFrame.closure;
				return Push(currentClosure.FreeVariables[freeIndex]);
			case OpCode.OpCurrentClosure:
				currentClosure = CurrentFrame.closure;
				return Push(currentClosure);
			case OpCode.OpArray:
				int numElements = Op.ReadUInt16([insBytes[1], insBytes[2]]); 
				var array = BuildArray(sp - numElements, sp);
				
				//sp = sp - numElements;
				for (int i = 0; i < numElements; i++)
				{
					//got to do it this weird slow way because undo-ability.
					Pop();
				}
				
				return Push(array);
			case OpCode.OpIndex:
				var index = Pop();
				var left = Pop();
				return RunIndexExpression(left, index);
		}
		return null;
		
	}
	public ScrubVMError? PreviousOne()
	{
		if (CurrentFrame.ip < 0)
		{
			if (Frames.TryPop(out var old))
			{
				//this is an undo of a Return. or, uh. Call.
				return new ScrubVMError("Can't undo out of the start of functions. yet.");
			}
			else
			{
				_state = VMState.Paused;
				Console.WriteLine("At beginning, can't undo!");
				return null;
			}
		}

		if (_state == VMState.Complete || _state == VMState.Error)
		{
			//todo: we assume we can undo an error we just reached going forward...? right? that should work?
			_state = VMState.Paused;
		}

		//this is the hot path. We actually care about performance.
		int[] ins;
		ScrubVMError? error = null;
		//ip is instructionPointer
	
		// CurrentFrame().ip;
		ip = CurrentFrame.ip;
		ins = CurrentFrame.Instructions();
		Console.WriteLine($"({CurrentLocation}) Undo: {Op.InstructionToString(ins[ip])}");
		var insBytes = BitConverter.GetBytes(ins[ip]);
		//fetch
		OpCode op = (OpCode)insBytes[0];
		//decode
		switch (op)
		{
			case OpCode.OpConstant:
				UnPush();
				var constIndex = Op.ReadUInt16([insBytes[1], insBytes[2]]); 
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpCall:
				var p = UnPush();//first, trash the result of the return.
				var numArgs = Op.ReadUInt8(insBytes[1]);
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return DeexecuteFunction(numArgs);
			case OpCode.OpClosure:
				var cIndex = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				UnPush();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				Log.RemoveOperation();
				return null;
			case OpCode.OpReturnValue:
				bool backwrds = 1 == Op.ReadUInt8(insBytes[1]); //is this a return from function or a finished-undoing-function (fstart)
				if (!backwrds) return null;
				//we already popped it off the stack in anti-call.
				_frame = PopFrame();
				//pop. The -1 gets rid of the function call too.
				//sp = _frame.basePointer - 1;
				CurrentFrame.ip--;
				Progress.DecrementCount();
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
			case OpCode.OpPow:
			case OpCode.OpMod:
				//binary ops are all the same. pop pop push. we reverse it with poppushpush.
				UnPush();//remove the result of the op
				UnPop();//restore the previous values. I think this will do it in the right order?
				UnPop();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				Log.RemoveOperation();
				return null;
			case OpCode.OpConcat:
				return new ScrubVMError($"Concatenation Operator Not Yet Implemented {CurrentLocation}");
			case OpCode.OpPop:
				UnPop();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpTrue:
				var o = UnPush();
				if ((Bool)o != True)
				{
					throw new VMException($"Reverse error (expected True, got {o}");
				}

				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpFalse:
				UnPush();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpEqual:
			case OpCode.OpNotEqual:
			case OpCode.OpGreaterThan:
				UnPush(); //remove the result of the op
				UnPop(); //restore the previous values
				UnPop();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				Log.RemoveOperation();
				return null;
			case OpCode.OpBang:
			case OpCode.OpNegate:
				//todo: can't we just run the negate operator again?
				UnPush();
				UnPop();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpJump:
				int pos = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				var reverse = Op.ReadUInt8(insBytes[3]) == 1;
				if (!reverse)
				{
					//don't jump forward jumps.
					CurrentFrame.ip--;
					Progress.DecrementCount();
					return null;
				}
				CurrentFrame.ip = pos - 1; //+1 when the loop ends :p
				Progress.DecrementCount();
				return null;
			case OpCode.OpJumpNotTruthy:
				//push truthy value w checked before back onto the stack.
				pos = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				reverse = Op.ReadUInt8(insBytes[3]) == 0;
				if (!reverse)
				{
					//don't jump forward jumps.
					CurrentFrame.ip--;
					Progress.DecrementCount();
					return null;
				}
				var condition = UnPop();
				bool jmp = !IsTruthy((Object)condition);
				_conditionalHistory[OpCode.OpJumpNotTruthy].Push(jmp);
				if (jmp)
				{
					CurrentFrame.ip = pos - 1;//pos-1 or pos+1? or pos? can't think, trial-and-error-ing.
				}

				Progress.DecrementCount();
				return null;
			case OpCode.OpNull:
				o = UnPush();
				if ((Null)o != Null)
				{
					throw new VMException($"Reverse error (expected True, got {o}");
				}
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpSetGlobal:
				var oldVal = _unstack.Pop();
				//o = UnPop();//set global doesn' push the value onto the stack, it leaves it there.
				//it will get removed by the next op, usually.
				var globalIndex = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				_globals[globalIndex] = (Object)oldVal;
				CurrentFrame.ip--;
				Progress.DecrementCount();
				Log.RemoveOperation();
				return null;
			case OpCode.OpSetLocal:
				oldVal = _unstack.Pop();
				var localIndex = Op.ReadUInt8(insBytes[1]);
				_frame = CurrentFrame;
				_stack[_frame.basePointer + (int)localIndex] = oldVal;
				CurrentFrame.ip--;
				Progress.DecrementCount();
				Log.RemoveOperation();
				return null;
			case OpCode.OpGetGlobal:
				globalIndex = Op.ReadUInt16([insBytes[1],insBytes[2]]);
				// Push(globals[globalIndex]);
				UnPush();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpGetLocal:
				localIndex = Op.ReadUInt8(insBytes[1]);
				//_frame = CurrentFrame();
				UnPush();
				// return Push(stack[_frame.basePointer + (int)localIndex]);
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpGetBuiltin:
				var builtInIndex = Op.ReadUInt8(insBytes[1]);
				UnPush();
				CurrentFrame.ip --;
				Progress.DecrementCount();
				//var def = Builtins.AllBuiltins[builtInIndex];
				return null;
				//return Push(def.Builtin);
			case OpCode.OpGetFree:
				var freeIndex = Op.ReadUInt8(insBytes[1]);
				//var currentClosure = CurrentFrame().closure;
				//return Push(currentClosure.FreeVariables[freeIndex]);
				UnPush();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpCurrentClosure:
				//currentClosure = CurrentFrame().closure;
				// return Push(currentClosure);
				UnPush();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
			case OpCode.OpArray:
				int numElements = Op.ReadUInt16([insBytes[1], insBytes[2]]);
				var array = BuildArray(sp - numElements, sp);
				sp = sp - numElements;
				UnPush();
				CurrentFrame.ip--;
				Progress.DecrementCount();
				return null;
				// return Push(array);
			case OpCode.OpIndex:
				UnPush();
				var left = UnPop();
				var index = UnPop();
				// return RunIndexExpression(left, index);
				CurrentFrame.ip --;
				Progress.DecrementCount();
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
				//uhhhh todo: allow us to define how this works
				UnPop();
				Log.RemoveOperation();
				//
				return null;
				break;
			default:
				break;
		}

		return new ScrubVMError($"Unable to UnExecute Function. Obj \"{callee}\" is not a function. {CurrentLocation}");

	}
	private ScrubVMError? ExecuteFunction(int numArgs)
	{
		//reach further down the stack to get the function, past the locals (arguments)
		//should be -1, why is it -2?
		var callee = _stack[sp - 1 - numArgs];
		switch (((Object)callee).GetType())
		{
			case ScrubType.Closure:
				return CallClosure((Closure)callee, numArgs);
			case ScrubType.Builtin:
				return CallBuiltin((Builtin)callee, numArgs);
			case ScrubType.Function:
				return new ScrubVMError($"Trying to Execute Function that isn't wrapped as a closure! {CurrentLocation}");
		}

		return new ScrubVMError($"Unable to Execute Function. Obj \"{callee}\" is not a function. {CurrentLocation}");
	}

	private ScrubVMError? CallBuiltin(Builtin fn, int numArgs)
	{
		var args = new Object[numArgs];
		//a lazy slice copy. fast tho.
		for (int i = 0; i < numArgs; i++)
		{
			args[i] = (Object)_stack[sp - numArgs + i];
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

		Log.AddOperation(Frames.Count, "built in function", args.ToDelimitedString(), "");

		return null;
	}

	private ScrubVMError? CallClosure(Closure cl, int numArgs)
	{
		if (numArgs != cl.CompiledFunction.NumArgs)
		{
			//todo: write tests for this.
			return new ScrubVMError($"Wrong number of arguments! {CurrentLocation}");
		}

		var frame = new Frame(cl,sp-numArgs);
		PushFrame(frame);
		sp = frame.basePointer + cl.CompiledFunction.NumArgs+cl.CompiledFunction.NumLocals;//give us a buffer of the number of local variables we will store in this area on the stack.
		Log.AddOperation(Frames.Count, cl.CompiledFunction.Name, "", "");
		return null;
	}

	private ScrubVMError? UnCallClosure(Closure cl, int numArgs)
	{
		if (numArgs != cl.CompiledFunction.NumArgs)
		{
			return new ScrubVMError($"Wrong number of arguments? But we are undo-ing? huh? {CurrentLocation}");
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
		Log.RemoveOperation();
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
			return new ScrubVMError($"Unable to index {lt} with {it} at {CurrentLocation}");
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
			elements[i - startIndex] = (Object)_stack[i];//todo: wrap this cast with error handling.
		}

		return new Array(elements);
	}

	private ScrubVMError? RunNegateOperator(OpCode op)
	{
		var operand = Pop();
		if (operand.GetType() == ScrubType.Int)
		{
			return Push(new Integer(-(operand as Integer).NativeInt));
		}

		return new ScrubVMError($"Unable to negate (-) {operand} at {CurrentLocation}");
	}

	private ScrubVMError? RunBangOperator(OpCode op)
	{
		var obj = Pop();

		if (obj == True)
		{
			return Push(False);
		}else if (obj == False)
		{
			return Push(True);
		}else if (obj == Null)
		{
			return Push(True);
		}
		else
		{
			if (op == OpCode.OpBang)
			{
				return new ScrubVMError(
					$"Can't run bang operator on {obj}. I don't know what our truthy  table is yet and I haven't decided. {CurrentLocation}");
			}else
			{
				return new ScrubVMError("Trying to run bang operator from incorrect state. Somnething went wrong earlier and the VM ended up here.");
			}

			//uh oh. warning? everythnig that is not false is truthy. Not so sure! todo: Investigate truthiness table.
			return Push(False);
		}
	}

	private ScrubVMError? RunComparisonOperation(OpCode op)
	{
		var r = Pop();
		var l = Pop();

		if (l.GetType() == ScrubType.Int && r.GetType() == ScrubType.Int)
		{
			return RunIntegerComparisonOperation(op, (Integer)l, (Integer)r);
		}else if (l.GetType() == ScrubType.Bool && r.GetType() == ScrubType.Bool)
		{
			//RunBooleanComparisonOperator:
			var lb = ((Bool)l).NativeBool;
			var rb = ((Bool)r).NativeBool;
			switch (op)
			{
				case OpCode.OpEqual:
					Log.AddOperation(Frames.Count,"==","",(lb == rb).ToString());
					return Push(NativeBoolToBooleanObject(lb == rb));
				case OpCode.OpNotEqual:
					Log.AddOperation(Frames.Count, "!=", "", (lb != rb).ToString());
					return Push(NativeBoolToBooleanObject(lb != rb));
				default:
					return new ScrubVMError($"Unknown Operator {{op}} for {l.GetType()} and {r.GetType()} at {CurrentLocation}");
			}
		}
		switch (op)
		{
			case OpCode.OpEqual:
				Log.AddOperation(Frames.Count, "==", "", (l == r).ToString());
				return Push(NativeBoolToBooleanObject(l == r));
			case OpCode.OpNotEqual:
				Log.AddOperation(Frames.Count, "!=", "", (l == r).ToString());
				return Push(NativeBoolToBooleanObject(l != r));
			default:
				return new ScrubVMError($"Unknown Operator {{op}} for {l.GetType()} and {r.GetType()} at {CurrentLocation}");
		}
	}

	private ScrubVMError? RunIntegerComparisonOperation(OpCode op, Integer a, Integer b)
	{
		switch (op)
		{
			//comparing native types here means we can't do false == 0, which scrub should evaluate to true.
			case OpCode.OpEqual:
				var result = a.NativeInt == b.NativeInt;
				Log.AddOperation(Frames.Count,"==",a.ToString()+", "+b.ToString(),result.ToString());
				return Push(NativeBoolToBooleanObject(result));
			case OpCode.OpNotEqual:
				result = a.NativeInt != b.NativeInt;
				Log.AddOperation(Frames.Count, "!=", a.ToString() + ", " + b.ToString(), result.ToString());
				return Push(NativeBoolToBooleanObject(result));
			case OpCode.OpGreaterThan:
				result = a.NativeInt > b.NativeInt;
				Log.AddOperation(Frames.Count, ">", a.ToString() + ", " + b.ToString(), result.ToString());
				return Push(NativeBoolToBooleanObject(result));
			default:
				return new ScrubVMError($"Unknown integer comparison Operator {op} {CurrentLocation}");
		}
	}

	private ScrubVMError? RunBitShiftOperation(OpCode op)
	{
		//todo: these are broken because, i think, bitwise things?
		var r = Pop();
		var l = Pop();
		string input = l.ToString();
		var rightType = r.GetType();
		
		if (op == OpCode.OpBitShiftLeft && rightType == ScrubType.Int)
		{
			l.BitShiftLeft(((Integer)r).NativeInt);
			Log.AddOperation(Frames.Count, $"Bitshift left {r}", input, l.ToString());
			return Push(l);
		}
		else if (op == OpCode.OpBitShiftRight && rightType == ScrubType.Int)
		{
			l.BitShiftLeft(((Integer)r).NativeInt);
			Log.AddOperation(Frames.Count, $"Bitshift right {r}", input, l.ToString());
			return Push(l);
		}
		else
		{
			//if opcode is not bitshift, which feel sunlikely enough to ignore it for now.
			if (op != OpCode.OpBitShiftLeft && op != OpCode.OpBitShiftRight)
			{
				return new ScrubVMError($"Trying to bit shift, but wrong operator? {CurrentLocation}");
			}
			else if(rightType != ScrubType.Int)
			{
				return new ScrubVMError($"Cannot Bitshift by non-integer amount. {CurrentLocation}");
			}
		}

		return null;
	}

	private ScrubVMError? RunBinaryOperation(OpCode op)
	{
		var r = Pop();
		var l = Pop();
		
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
		
		return new ScrubVMError($"Unsupported types ({l.GetType()},{r.GetType()}) for operation {op} at {CurrentLocation}");
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
				return new ScrubVMError($"Unsupported binary operation {op} for 2 booleans at {CurrentLocation}");
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
			case OpCode.OpMod:
				result = left % right;
				break;
			case OpCode.OpPow:
				result = new Integer((int)Math.Pow(left.NativeInt, right.NativeInt));
				break;
			case OpCode.OpConcat:
				//todo: optimize these to use internal casts, instead of round-trip through native casts.
				//todo: should 1++2 return 12 or "12". right now it's "12", which i think is right.
				return RunBinaryStringOperation(op, new String(left.ToString()), new String(right.ToString()));
			default:
				return new ScrubVMError($"Unkown Integer Operation {op} at {CurrentLocation}");
		}

		Log.AddOperation(Frames.Count, op.ToString(), left.ToString() + ", " + right.ToString(), result.ToString());
		return Push(result);
	}

	private ScrubVMError? RunBinaryStringOperation(OpCode op, String left, String right)
	{
		String result = new String("");
		switch (op)
		{
			case OpCode.OpAdd:
				result = new String(left, right);
				Log.AddOperation(Frames.Count, "add strings", "", result.ToString());
				break;
			case OpCode.OpConcat:
				result = new String(left, right);
				Log.AddOperation(Frames.Count, "concat strings", "", result.ToString());
				break;
			default:
				return new ScrubVMError($"Unkown Integer Operation {op} at {CurrentLocation}");
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
	private ScrubVMError? Push(Object o)
	{
		if (sp >= StackSize)
		{
			return new ScrubVMError("Stack Overflow!");
		}
		
		_stack[sp] = o;
		sp++;
		return null;
	}

	

	private ScrubVMError? PushClosure(int closureConstIndex, int numFree)
	{
		var constant = _constants[closureConstIndex];//todo: type check throw error.
		var fn = (Function)constant;

		//pull the free objects off of the stack and shove them into the closure object at runtime.
		//the order is important.
		var free = new Object[numFree];
		for (var i = 0; i < free.Length; i++)
		{
			free[i] = (Object)_stack[sp - numFree + i];
		}

		sp = sp - numFree;

		var closure = new Closure(fn,free);//runtime closure from function! That's the whole point of closures!
		Push(closure);
		return null;
	}

	private Object Pop()
	{
		//this is why you don't catch edge cases, but fail them.
		//i spent half an hour chasing issues elsewhere when the thing that put me on the right track
		//was that this was silently ignoreing an error
		//i put a console.write, but the test suite had it below the stack trace and i didn't scroll down)
		// if (sp == 0)
		// {
		// 	return null;
		// 	return new ScrubVMError("Warning, popping with nothing to pop.");
		// 	return null;
		// }
		
		var o = _stack[sp - 1];
		sp--;
		_unstack.Push(o);
		usp++;
		return o;
	}

	private Object UnPop()
	{
		if (sp >= StackSize)
		{
			//todo: error objects.
			throw new VMException("Stack Overflow!");
			//return new ScrubVMError("Stack Overflow!");
		}

		var o = _unstack.Pop();
		usp--;
		_stack[sp] = o;
		sp++;
		return o;
	}
	private Object UnPush()
	{
		var o = _stack[sp - 1];
		sp--;
		return o;
	}
	public Object LastPopped()
	{
		return _stack[sp];
	}

	public Location CurrentLocation => CurrentFrame.GetLocation();
	public Frame CurrentFrame => Frames.Peek();

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
	
}