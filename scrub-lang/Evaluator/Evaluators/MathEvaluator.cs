using System.Xml.Xsl;
using scrub_lang.Memory;
using scrub_lang.Parser;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Evaluator.Evaluators;

public static class MathEvaluator
{
	public static Result Evaluate(Evaluator evaluator, BinaryMathExpression bme)
	{
		var left = evaluator.Eval(bme.Left);
		var right = evaluator.Eval(bme.Right);
		if (left.HasError)
		{
			return left;
		}

		if (right.HasError)
		{
			return right;
		}
		//
		switch (bme.Operator)
		{
			case TokenType.Plus:
				return EvalPlus(left.ScrubObject, right.ScrubObject);
			case TokenType.Minus:
				return EvalMinus(left.ScrubObject, right.ScrubObject);
			case TokenType.Multiply:
				return EvalProduct(left.ScrubObject, right.ScrubObject);
			case TokenType.Division:
				return EvalDivide(left.ScrubObject, right.ScrubObject);
			case TokenType.PowerOf:
				return EvalPower(left.ScrubObject, right.ScrubObject);
			case TokenType.Modulo:
				return new Result(new ScrubRuntimeError("Modulo not yet implemented :p"));
		}

		return new Result(
			new ScrubRuntimeError($"Unable to do operation {bme.Operator} on {right.ScrubObject} and {left.ScrubObject}"));
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

}