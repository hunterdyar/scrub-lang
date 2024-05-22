using System.Linq.Expressions;
using System.Net.Security;
using Microsoft.VisualBasic.CompilerServices;
using scrub_lang.Evaluator;
using scrub_lang.Memory;
using scrub_lang.Objects;
using scrub_lang.Parser;
using scrub_lang.Tokenizer.Tokens;
using ConditionalExpression = scrub_lang.Parser.ConditionalExpression;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.Compiler;

public class Compiler
{
	private List<byte> instructions = new List<byte>();
	private List<Object> constants = new List<Object>();//ScrubObject?
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
					return new ScrubCompilerError(error.Message);
				}

				Emit(OpCode.OpPop);//these values are unused. All exresssions are unused unless they go into some operator or such.
				//it's a little weird because everything is an expression, but... after what would be a statement, we clean up the leftover value that the expressions created.
			}
			return null;
		}else if (expression is ExpressionGroupExpression block)
		{
			for (int i = 0; i < block.Expressions.Count; i++)
			{
				var err = Compile(block.Expressions[i]);
				if (err != null)
				{
					return err;
				}

				if (i < block.Expressions.Count - 1)
				{
					//remove the value from the expression we just called.... which might not have a value? hmmm. shite.
					//our expression leaves us with one nice value at the end, which is what expression blocks become: their last value.
					Emit(OpCode.OpPop);
				}
			}
		}
		
		else if (expression is BinaryOperatorExpressionBase bin)
		{
			if (bin.Operator == TokenType.LessThan)
			{
				var rightErr = Compile(bin.Right);
				if (rightErr != null)
				{
					return new ScrubCompilerError(rightErr.Message);
				}
				
				var leftErr = Compile(bin.Left);
				if (leftErr != null)
				{
					return new ScrubCompilerError(leftErr.Message);
				}

				// a<b gets sent to the stack as b>a. we do right before left (above), then a > op
				Emit(OpCode.OpGreaterThan);
				return null;
			}
			var leftError = Compile(bin.Left);
			if (leftError != null)
			{
				return new ScrubCompilerError(leftError.Message);
			}

			var rightError = Compile(bin.Right);
			if (rightError != null)
			{
				return new ScrubCompilerError(rightError.Message);
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
				case TokenType.EqualTo:
					Emit(OpCode.OpEqual);
					break;
				case TokenType.NotEquals:
					Emit(OpCode.OpNotEqual);
					break;
				case TokenType.GreaterThan:
					Emit(OpCode.OpGreaterThan);
					break;
				default:
					return new ScrubCompilerError($"Unable to Compile Operator {Token.OperatorToString(bin.Operator)}");
			}

			return null;
		}else if (expression is PrefixExpression pre)
		{
			var error = Compile(pre.Right);//should leave an integer, true, or false on the stack.
			if (error != null)
			{
				return new ScrubCompilerError(error.Message);
			}

			switch (pre.Op)
			{
				case TokenType.Minus:
					Emit(OpCode.OpNegate);
					break;
				case TokenType.Bang:
					Emit(OpCode.OpBang);
					break;
				default:
					return new ScrubCompilerError($"Unable to Compile Prefix Operator {pre.Op}");
			}

			return null;
		}else if (expression is PostfixExpressionBase post)
		{
			var leftError = Compile(post.Left);
			if (leftError != null)
			{
				return new ScrubCompilerError(leftError.Message);
			}

			return null;
		}
		//other base expressions...
		
		//code-flow
		else if (expression is ConditionalExpression condExpr)
		{
			var err = Compile(condExpr.Conditional);
			if (err != null)
			{
				return err;
			}

			Emit(OpCode.OpJumpNotTruthy, 9999);//bogus value, let's fix it later.

			err = Compile(condExpr.Consequence);
			if (err != null)
			{
				return err;
			}

			//not using this as a return value, so pop it.
			Emit(OpCode.OpPop);
			//todo
			throw new NotImplementedException("This is where I stopped working for a bit");
		}
		//should we have a literalExpressionBase?
		else if (expression is NumberLiteralExpression numLitExp)
		{
			//todo: we are only supporting int's to start.
			var number = numLitExp.GetScrubObject();
			//create a new instruction. The operatand is the index of number in our constants pool, basically.
			Emit(OpCode.OpConstant, AddConstant(number));
			return null;
		}else if (expression is BoolLiteralExpression boolLitExp)
		{
			Emit(boolLitExp.Literal ? OpCode.OpTrue : OpCode.OpFalse);
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
	
	/// <returns>Returns this constants insdex in the constants pool.</returns>
	public int AddConstant(Object obj)
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