using System.Xml.Xsl;
using scrub_lang.Memory;
using scrub_lang.Parser;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Evaluator.Evaluators;

public static class MathEvaluator
{
	public static Result Evaluate(Evaluator evaluator, BinaryMathExpression bme, Environment environment)
	{
		environment.Ascend(Token.OperatorToString(bme.Operator));
		var left = evaluator.Eval(bme.Left, environment);
		var right = evaluator.Eval(bme.Right, environment);

		if (left.HasError)
		{
			return left;
		}

		if (right.HasError)
		{
			return right;
		}

		Result? res = null;
		switch (bme.Operator)
		{
			case TokenType.Plus:
				res = EvalPlus(left.ScrubObject, right.ScrubObject);
				break;
			case TokenType.Minus:
				res = EvalMinus(left.ScrubObject, right.ScrubObject);
				break;
			case TokenType.Multiply:
				res = EvalProduct(left.ScrubObject, right.ScrubObject);
				break;
			case TokenType.Division:
				res = EvalDivide(left.ScrubObject, right.ScrubObject);
				break;
			case TokenType.PowerOf:
				res = EvalPower(left.ScrubObject, right.ScrubObject);
				break;
			case TokenType.Modulo:
				res = EvalModulo(left.ScrubObject, right.ScrubObject);
				break;
		}

		if (res == null)
		{
			res = new Result(
				new ScrubRuntimeError(
					$"Unable to do operation {bme.Operator} on {right.ScrubObject} and {left.ScrubObject}"));
		}
		
		environment.StepExecution(bme, res, $"{left.ScrubObject} {Token.OperatorToString(bme.Operator)} {right.ScrubObject}");
		environment.Descend(res);
		return res;
		
	}

	private static Result EvalPlus(ScrubObject left, ScrubObject right)
	{
		if (left.ScrubType == ScrubType.sInt)
		{
			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeInt() + right.ToNativeInt()));
			}
			
			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeInt() + (double)right.ToNativeDouble()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeInt() + (int)right.ToNativeUInt()));
			}
		}
		
		if (left.ScrubType == ScrubType.sDouble)
		{
			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeDouble() + (double)right.ToNativeDouble()));
			}
			
			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() + (double)right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() + (int)right.ToNativeUInt()));
			}
		}

		if (left.ScrubType == ScrubType.sUint)
		{
			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeUInt() + (int)right.ToNativeUInt()));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject((int)left.ToNativeUInt() + right.ToNativeInt()));
			}
			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeUInt() + (double)right.ToNativeDouble()));
			}
			
		}

		return new Result(null,
			new ScrubRuntimeError($"Unable to add objects of type {left.ScrubType} and {right.ScrubType}"));
	}
	private static Result EvalMinus(ScrubObject left, ScrubObject right)
	{
		if (left.ScrubType == ScrubType.sInt)
		{
			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeInt() - right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeInt() - (double)right.ToNativeDouble()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeInt() - (int)right.ToNativeUInt()));
			}
		}

		if (left.ScrubType == ScrubType.sDouble)
		{
			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeDouble() - (double)right.ToNativeDouble()));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() - (double)right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() - (int)right.ToNativeUInt()));
			}
		}

		if (left.ScrubType == ScrubType.sUint)
		{
			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeUInt() - (int)right.ToNativeUInt()));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject((int)left.ToNativeUInt() - right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeUInt() - (double)right.ToNativeDouble()));
			}

		}

		return new Result(null,
			new ScrubRuntimeError($"Unable to subtract objects of type {left.ScrubType} and {right.ScrubType}"));
	}

	private static Result EvalProduct(ScrubObject left, ScrubObject right)
	{
		if (left.ScrubType == ScrubType.sInt)
		{
			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeInt() * right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeInt() * (double)right.ToNativeDouble()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeInt() * (int)right.ToNativeUInt()));
			}
		}

		if (left.ScrubType == ScrubType.sDouble)
		{
			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeDouble() * (double)right.ToNativeDouble()));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() * (double)right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() * (int)right.ToNativeUInt()));
			}
		}

		if (left.ScrubType == ScrubType.sUint)
		{
			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeUInt() * (int)right.ToNativeUInt()));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject((int)left.ToNativeUInt() * right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeUInt() * (double)right.ToNativeDouble()));
			}

		}

		return new Result(null,
			new ScrubRuntimeError($"Unable to multiply objects of type {left.ScrubType} and {right.ScrubType}"));
	}

	private static Result EvalDivide(ScrubObject left, ScrubObject right)
	{
		if (left.ScrubType == ScrubType.sInt)
		{
			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeInt() / right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeInt() / (double)right.ToNativeDouble()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeInt() / (int)right.ToNativeUInt()));
			}
		}

		if (left.ScrubType == ScrubType.sDouble)
		{
			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeDouble() / (double)right.ToNativeDouble()));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() / (double)right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() / (int)right.ToNativeUInt()));
			}
		}

		if (left.ScrubType == ScrubType.sUint)
		{
			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeUInt() / (int)right.ToNativeUInt()));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject((int)left.ToNativeUInt() / right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeUInt() / (double)right.ToNativeDouble()));
			}

		}

		return new Result(null,
			new ScrubRuntimeError($"Unable to divide objects of type {left.ScrubType} and {right.ScrubType}"));
	}

	private static Result EvalPower(ScrubObject left, ScrubObject right)
	{
		if (left.ScrubType == ScrubType.sInt)
		{
			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject((int)Math.Pow(left.ToNativeInt(),right.ToNativeInt())));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject(Math.Pow((double)left.ToNativeInt(), (double)right.ToNativeDouble())));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject((int)Math.Pow(left.ToNativeInt(), right.ToNativeUInt())));
			}
		}

		if (left.ScrubType == ScrubType.sDouble)
		{
			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject(Math.Pow((double)left.ToNativeDouble(),(double)right.ToNativeDouble())));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(Math.Pow((double)left.ToNativeDouble(), (double)right.ToNativeInt())));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(Math.Pow((double)left.ToNativeDouble(), (double)right.ToNativeUInt())));
			}
		}

		if (left.ScrubType == ScrubType.sUint)
		{
			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject((int)Math.Pow(left.ToNativeUInt(), right.ToNativeInt())));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject(Math.Pow((double)left.ToNativeUInt(),
					(double)right.ToNativeDouble())));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject((int)Math.Pow(left.ToNativeUInt(), right.ToNativeUInt())));
			}
		}

		return new Result(null,
			new ScrubRuntimeError($"Unable to take power of objects of type {left.ScrubType} and {right.ScrubType}"));
	}

	private static Result EvalModulo(ScrubObject left, ScrubObject right)
	{
		if (left.ScrubType == ScrubType.sInt)
		{
			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeInt() % right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeInt() % (double)right.ToNativeDouble()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeInt() % (int)right.ToNativeUInt()));
			}
		}

		if (left.ScrubType == ScrubType.sDouble)
		{
			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeDouble() % (double)right.ToNativeDouble()));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() % (double)right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeDouble() % (int)right.ToNativeUInt()));
			}
		}

		if (left.ScrubType == ScrubType.sUint)
		{
			if (right.ScrubType == ScrubType.sUint)
			{
				return new Result(new ScrubObject(left.ToNativeUInt() % (int)right.ToNativeUInt()));
			}

			if (right.ScrubType == ScrubType.sInt)
			{
				return new Result(new ScrubObject((int)left.ToNativeUInt() % right.ToNativeInt()));
			}

			if (right.ScrubType == ScrubType.sDouble)
			{
				return new Result(new ScrubObject((double)left.ToNativeUInt() % (double)right.ToNativeDouble()));
			}

		}

		return new Result(null,
			new ScrubRuntimeError($"Unable to modulo objects of type {left.ScrubType} and {right.ScrubType}"));
	}

}