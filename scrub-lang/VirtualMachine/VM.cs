using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Xml.Xsl;
using scrub_lang.Compiler;
using scrub_lang.Evaluator;
using scrub_lang.Objects;
using scrub_lang.Parser;
using Array = scrub_lang.Objects.Array;
using Environment = scrub_lang.Evaluator.Environment;
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
		
	public ByteCode ByteCode { get; }

	public Stack<Frame> Frames = new Stack<Frame>();//todo: replace this with pre-allocated array (for SPEeeEEed)
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
	
	public VM(ByteCode byteCode)
	{
		ByteCode = byteCode;//keep a copy.
		//instructions
		var mainFunction = new Function(byteCode.Instructions);
		var mainFrame = new Frame(mainFunction);
		Frames = new Stack<Frame>();
		Frames.Push(mainFrame);
		
		constants = byteCode.Constants.ToList();
		stack = new object[StackSize];//todo: will this become a different base type? 
		unstack = new object[StackSize];
		globals = new Object[GlobalsSize];
		sp = 0;
		usp = 0;
	}

	public VM(ByteCode byteCode, Object[] globalsStore)
	{
		ByteCode = byteCode; //keep a copy.
		//instructions
		var mainFunction = new Function(byteCode.Instructions);
		var mainFrame = new Frame(mainFunction);
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
		//this is the hot path. We actually care about performance.
		int ip = 0;
		byte[] ins;
		OpCode op;
		//ip is instructionPointer
		while (CurrentFrame().ip < CurrentFrame().Instructions().Length-1)
		{
			CurrentFrame().ip++;//increment at start instead of at end because of all the returns. THis is why we init the frame with an ip of -1.

			//fetch -> decode -> execute
			ip = CurrentFrame().ip;
			ins = CurrentFrame().Instructions();
			//fetch
			op = (OpCode)ins[ip];
			
			//decode
			switch (op)
			{
				case OpCode.OpConstant:
					//todo: big/little endian
					var constIndex = BitConverter.ToInt16([ins[ip + 2], ins[ip+1]]);//todo: write fast ReadUInt16 function and handle big/little endian
					CurrentFrame().ip += 2;//increase the number of bytes re read to decode the operands. THis leaves the next instruction pointing at an OpCode.
					
					//execute
					var error = Push(constants[constIndex]);
					if (error != null)
					{
						return error;
					}
					break;
				case OpCode.OpCall:
					var fnobj = stack[sp - 1];
					if (fnobj is not Function fun)
					{
						return new ScrubVMError("function calling a not-a-function");
					}
					var frame = new Frame(fun);
					PushFrame(frame);
					break;
				case OpCode.OpReturnValue:
					var returnValue = Pop();
					PopFrame();
					Pop();//
					error = Push(returnValue);
					if (error != null)
					{
						return error;
					}
					break;
				case OpCode.OpAdd:
				case OpCode.OpSubtract:
				case OpCode.OpMult:
				case OpCode.OpDivide:
				
					error = RunBinaryOperation(op);
					if (error != null)
					{
						return error;
					}
					break;
				case OpCode.OpConcat:
					return new ScrubVMError("Concatenation Operator Not Yet Implemented");
					//runBinaryOperation, do same call as the add/sub/mult/div
				case OpCode.OpPop:
					Pop();
					break;
				case OpCode.OpTrue:
					Push(True);
					break;
				case OpCode.OpFalse:
					Push(False);
					break;
				case OpCode.OpEqual:
				case OpCode.OpNotEqual:
				case OpCode.OpGreaterThan:
					error = RunComparisonOperation(op);
					if (error != null)
					{
						return error;
					}
					break;
				case OpCode.OpBang:
					error = RunBangOperator(op);
					if (error != null)
					{
						return error;
					}
					break;
				case OpCode.OpNegate:
					error = RunNegateOperator(op);
					if (error != null)
					{
						return error;
					}
					break;
				case OpCode.OpJump:
					int pos = Op.ReadUInt16([ins[ip+1],ins[ip+2]]);
					CurrentFrame().ip = pos - 1;//+1 when the loop ends :p
					break;
				case OpCode.OpJumpNotTruthy:
					pos = Op.ReadUInt16( [ins[ip+1], ins[ip+2]]);
					CurrentFrame().ip += 2;//skipPastTheJump if we are truthy
					var condition = PopScrubObject();
					if (!IsTruthy(condition))
					{
						CurrentFrame().ip = pos - 1;
					}
					break;
				case OpCode.OpNull:
					Push(Null);
					break;
				case OpCode.OpSetGlobal:
					var globalIndex = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]);
					CurrentFrame().ip += 2;
					globals[globalIndex] = PopScrubObject();
					break;
				case OpCode.OpGetGlobal:
					globalIndex = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]);
					CurrentFrame().ip += 2;
					Push(globals[globalIndex]);
					break;
				case OpCode.OpArray:
					int numElements = Op.ReadUInt16([ins[ip + 1], ins[ip + 2]]);
					CurrentFrame().ip += 2;

					var array = BuildArray(sp - numElements, sp);
					sp = sp - numElements;
					var err = Push(array);
					if (err != null)
					{
						return err;
					}
					break;
				case OpCode.OpIndex:
					var index = PopScrubObject();
					var left = PopScrubObject();
					err = RunIndexExpression(left, index);
					if (err != null)
					{
						return err;
					}

					break;
			}
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
			}else if (i < 8)
			{
				return Push(new Bool(((left.Bytes[BitConverter.IsLittleEndian ? 3 : 0] >> i) & 1) != 0));
			}else if (i < 16)
			{
				return Push(new Bool(((left.Bytes[BitConverter.IsLittleEndian ? 2 : 1] >> (i-8)) & 1) != 0));
			}else if (i < 24)
			{
				return Push(new Bool(((left.Bytes[BitConverter.IsLittleEndian ? 1 : 2] >> (i - 16)) & 1) != 0));
			}
			else if (i < 32)
			{
				return Push(new Bool(((left.Bytes[BitConverter.IsLittleEndian ? 0 : 3] >> (i - 24)) & 1) != 0));
			}
			else
			{
				return Push(Null);
			}
		}
		else if (lt != ScrubType.Null && it == ScrubType.Int) {
			//get the byte data.
			var i = ((Integer)index).NativeInt;
			var max = left.Bytes.Length - 1;
			if (i < 0 || i > max)
			{
				return Push(Null);
			}
			//todo: we don't have a byte type... or a char type. Keep your UTF8 lookup tables handly!
			return Push(new Integer(left.Bytes[i]));
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
		}

		//we do this after the direct checks, because I think it will be faster this way.
		//It handles all of the other objects/pairings, using strings made from nativeToString.
		if (op == OpCode.OpConcat)
		{
			return RunConcatOperation(op, l, r);
		}
		
		return new ScrubVMError($"Unsupported types for operation {op}");
	}

	private ScrubVMError? RunBinaryIntegerOperation(OpCode op, Integer left, Integer right)
	{
		Integer result = new Integer(0);
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
				result = new String(left.Bytes,right.Bytes);
				break;
			case OpCode.OpConcat:
				result = new String(left.Bytes, right.Bytes);
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
		usp = usp > 0 ? usp-1: usp;//decrease but floor at 0.
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

	public Object PopScrubObject()
	{
		//basically calling pop, but faster to just paste. (i know!? wild).
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
	public void PushFrame(Frame f) { Frames.Push(f);}
	public Frame PopFrame() => Frames.Pop();
	
	

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
		var t = new Tokenizer.Tokenizer(input);
		var p = new Parser.Parser(t);
		return p.ParseProgram();
	}
}