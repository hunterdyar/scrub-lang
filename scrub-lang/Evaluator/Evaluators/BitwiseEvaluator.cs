using scrub_lang.Memory;
using scrub_lang.Parser;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Evaluator.Evaluators;

public static class BitwiseEvaluator
{
	public static Result Evaluate(Evaluator evaluator, BinaryBitwiseExpression bbe, Environment environment)
	{
		var left = evaluator.Eval(bbe.Left, environment);
		var right = evaluator.Eval(bbe.Right,environment);
		if (left.HasError)
		{
			return left;
		}

		if (right.HasError)
		{
			return right;
		}

		Result? res = null;

		switch (bbe.Operator)
		{
			case TokenType.BitwiseAnd:
				res = EvalAnd(left.ScrubObject, right.ScrubObject);
				break;
			case TokenType.BitwiseOr:
				res = new Result(new ScrubRuntimeError("Bitwise Or Not Implemented Yet, sorry."));
				break;
			case TokenType.BitwiseXOR:
				res = new Result(new ScrubRuntimeError("Bitwise XOR not implemented yet. oops."));
				break;
			case TokenType.BitwiseLeftShift:
				res = new Result(new ScrubRuntimeError("Bitwise << not implemented yet. oops."));
				break;
			case TokenType.BitwiseRightShift:
				res = new Result(new ScrubRuntimeError("Bitwise >> not implemented yet. oops."));
				break;
		}
		
		return res;
	}

	private static Result? EvalAnd(ScrubObject left, ScrubObject right)
	{
		if (left.ScrubType == ScrubType.sBool && right.ScrubType == ScrubType.sBool)
		{
			var r = left.ToNativeBool() & right.ToNativeBool();
			return new Result(new ScrubObject(r));
		} else if (left.ScrubType == ScrubType.sInt && right.ScrubType == ScrubType.sInt)
		{
			var r = left.ToNativeInt() & right.ToNativeInt();
			return new Result(new ScrubObject(r));
		}else if (left.ScrubType == ScrubType.sUint && right.ScrubType == ScrubType.sUint)
		{
			var r = left.ToNativeUInt() & right.ToNativeUInt();
			return new Result(new ScrubObject(r));
		}
		
		//brute force the operation by bitwise comparing the byte data, and using the type of the longer type.
		//this is extremely silly and extremely unclear.
		//Just because you can, doesn't mean you should! A Warning in the exec-log should go here.
		
		var x = left.Data.Length;
		var t = left.ScrubType;
		if (right.Data.Length > x)
		{
			x = right.Data.Length;
			t = right.ScrubType;
		}
		var rBytes= new byte[x];
		for (int i = 0; i < x; i++)
		{
			byte l = 0;
			byte r = 0;
			if (x < left.Data.Length)
			{
				l = left.Data[i];
			}

			if (x < right.Data.Length)
			{
				l = right.Data[i];
			}

			rBytes[i] = (byte)(l & r);
		}
		return new Result(new ScrubObject(rBytes, t));

	}
}