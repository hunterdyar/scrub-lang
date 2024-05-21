using System.Diagnostics;
using System.Text;
using scrub_lang.Code;
using scrub_lang.Compiler;
using scrub_lang.Evaluator;
using scrub_lang.Parser;

namespace scrub_lang.VirtualMachine;

public class VM
{
	public const int StackSize = 2048;
	public ByteCode ByteCode { get; }

	private List<byte> instructions;
	private List<object> constants;
	private object[] stack;//I do think I want to replace this with my own base ScrubObject object, instead of C# object.
	
	//StackPointer will always point to the next free slot in the stack. Top element will be sp-1
	//we put something in SP, then increment it.
	private int sp;//stack pointer
	public VM(ByteCode byteCode)
	{
		ByteCode = byteCode;//keep a copy.
		
		instructions = byteCode.Instructions.ToList();
		constants = byteCode.Constants.ToList();
		stack = new object[StackSize];
		sp = 0;
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
			}
		}
		return null;
	}

	private ScrubVMError? RunBinaryOperation(OpCode op)
	{
		var r = Pop();
		var l = Pop();
		
		//var leftType = left.Type();
		//var rightTYpe = right.Type();
		
		//hmmm
		if (l is int li && r is int ri)
		{
			return RunBinaryintegerOperation(op,  li, ri);
		}

		return new ScrubVMError("Unsupported types for binary operation {op}");
	}

	private ScrubVMError? RunBinaryintegerOperation(OpCode op, int left, int right)
	{
		long result = 0;
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
		return Push((int)result);
	}

	private ScrubVMError Push(object o)
	{
		if (sp >= StackSize)
		{
			return new ScrubVMError("Stack Overflow!");
		}

		stack[sp] = o;
		sp++;
		return null;
	}

	private object Pop()
	{
		var o = stack[sp - 1];
		sp--;
		return o;
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

	

	public static IExpression Parse(string input)
	{
		var t = new Tokenizer.Tokenizer(input);
		var p = new Parser.Parser(t);
		return p.ParseProgram();
	}
}