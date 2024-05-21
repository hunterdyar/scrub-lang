using Microsoft.VisualBasic.CompilerServices;
using scrub_lang.Code;
using scrub_lang.Evaluator;
using scrub_lang.Memory;
using scrub_lang.Parser;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Compiler;

public class Compiler
{
	private List<byte> instructions = new List<byte>();
	private List<object> constants = new List<object>();//ScrubObject?
	public Compiler()
	{
		
	}
	public ScrubCompilerError? Compile(IExpression expression)
	{
		if (expression is ProgramExpression pe)
		{
			foreach (var e in pe.Expressions)
			{
				var error = Compile(e);
				if (error != null)
				{
					throw new CompileException(error.Message);
				}

				Emit(OpCode.OpPop);
			}
			return null;
		}else if (expression is BinaryOperatorExpressionBase bin)
		{
			var leftError = Compile(bin.Left);
			if (leftError != null)
			{
				throw new CompileException(leftError.Message);
			}

			var rightError = Compile(bin.Right);
			if (rightError != null)
			{
				throw new CompileException(rightError.Message);
			}

			switch (bin.Operator)
			{
				case TokenType.Plus:
					Emit(OpCode.OpAdd);
					break;
				case TokenType.Minus:
					Emit(OpCode.OpSubtract);
					break;
				case TokenType.Multiply:
					Emit(OpCode.OpMult);
					break;
				case TokenType.Division:
					Emit(OpCode.OpDivide);
					break;
				default:
					return new ScrubCompilerError($"Unable to Compile Operator {Token.OperatorToString(bin.Operator)}");
			}

			return null;
		}else if (expression is PrefixExpression pre)
		{
			var error = Compile(pre.Right);
			if (error != null)
			{
				throw new CompileException(error.Message);
			}

			return null;
		}else if (expression is PostfixExpressionBase post)
		{
			var leftError = Compile(post.Left);
			if (leftError != null)
			{
				throw new CompileException(leftError.Message);
			}

			return null;
		}
		//other base expressions...
		
		//should we have a literalExpressionBase?
		else if (expression is NumberLiteralExpression numLitExp)
		{
			//todo: we are only supporting int's to start.
			int number = numLitExp.AsInt;
			//create a new instruction. The operatand is the index of number in our constants pool, basically.
			Emit(OpCode.OpConstant, AddConstant(number));
			return null;
		}

		return new ScrubCompilerError($"Unable to parse expression {expression}. Probably not implemented the type yet.");
	}

	public int Emit(OpCode op, params int[] operands)
	{
		var ins = Op.Make(op, operands);
		var pos = AddInstruction(ins);
		return pos;
	}

	public int AddInstruction(byte[] instruction)
	{
		var posNewInstruction = instructions.Count;
		foreach (byte b in instruction)
		{
			instructions.Add(b);
		}

		return posNewInstruction;
	}
	
	/// <returns>Returns this constants index in the constants pool.</returns>
	public int AddConstant(object obj)
	{
		this.constants.Add(obj);
		return this.constants.Count - 1;
	}
	//this is what gets passed to the VM.
	public ByteCode ByteCode()
	{
		return new ByteCode(instructions.ToArray(),constants.ToArray());
	}


}