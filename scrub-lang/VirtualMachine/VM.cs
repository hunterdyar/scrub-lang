using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Xsl;
using scrub_lang.Compiler;
using scrub_lang.Evaluator;
using scrub_lang.Objects;
using scrub_lang.Parser;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.VirtualMachine;

public class VM
{
	public const int StackSize = 2048;
	
	//some consts because why have many number when two number do.
	public static readonly Bool True = new Bool(true);
	public static readonly Bool False = new Bool(false);
	public ByteCode ByteCode { get; }

	private List<byte> instructions;
	private List<Object> constants;
	private object[] stack;//I do think I want to replace this with my own base ScrubObject? Not sure.

	private object[] unstack;//I am extremely split on calling this the UnStack or the AntiStack.
	
	//StackPointer will always point to the next free slot in the stack. Top element will be sp-1
	//we put something in SP, then increment it.
	private int sp;//stack pointer
	private int usp; //unstack pointer.
	
	public VM(ByteCode byteCode)
	{
		ByteCode = byteCode;//keep a copy.
		
		instructions = byteCode.Instructions.ToList();
		constants = byteCode.Constants.ToList();
		stack = new object[StackSize];//todo: will this become a different base type? 
		unstack = new object[StackSize];
		sp = 0;
		usp = 0;
	}

	public ScrubVMError? Run()
	{
		//this is the hot path. We actually care about performance.
		
		//ip is instructionPointer
		for (int ip = 0; ip < instructions.Count; ip++)
		{
			//fetch -> decode -> execute
			
			//fetch
			var op = (OpCode)instructions[ip];
			//decode
			switch (op)
			{
				case OpCode.OpConstant:
					//todo: big/little endian
					var constIndex = BitConverter.ToInt16([instructions[ip + 2], instructions[ip+1]]);//todo: write fast ReadUInt16 function and handle big/little endian
					ip += 2;//increase the number of bytes re read to decode the operands. THis leaves the next instruction pointing at an OpCode.
					
					//execute
					var error = Push(constants[constIndex]);
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
			}
		}
		return null;
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
		//var leftType = left.Type();
		//var rightTYpe = right.Type();
		
		//hmmm. Feeels like we are casting twice. But, hey, at least we are sure it will work.
		//I low-key want to use a single type for all of the underlying objects, and just switch case? is that stupid? It feels stupid, but casting everywhere does too. 
		//I thought aobut it and yeah its stupid. Just weird to write a dynamic-ish language in a staticly typed one no matter which way you slice it.
		if(l.GetType() == ScrubType.Int && r.GetType() == ScrubType.Int)
		{
			return RunBinaryintegerOperation(op,  (Integer)l, (Integer)r);
		}

		return new ScrubVMError($"Unsupported types for operation {op}");
	}

	private ScrubVMError? RunBinaryintegerOperation(OpCode op, Integer left, Integer right)
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
			default:
				return new ScrubVMError($"Unkown Integer Operation {op}");
		}
		return Push(result);
	}

	private ScrubVMError Push(object o)
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

	// public object StackTop()
	// {
	// 	if (sp == 0)
	// 	{
	// 		return null;
	// 	}
	// 	return stack[sp-1];
	// }

	public object LastPopped()
	{
		return stack[sp];
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