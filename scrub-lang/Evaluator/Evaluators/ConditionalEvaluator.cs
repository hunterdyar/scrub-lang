using scrub_lang.Memory;
using scrub_lang.Parser;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Evaluator.Evaluators;

public class ConditionalEvaluator
{
	public static Result Evaluate(Evaluator evaluator, BinaryConditionalExpression bce, Environment environment)
	{
		var left = evaluator.Eval(bce.Left, environment);
		var right = evaluator.Eval(bce.Right, environment);
		
		if (left.HasError)
		{
			return left;
		}

		if (right.HasError)
		{
			return right;
		}

		Result? res = null;
		switch (bce.Operator)
		{
			case TokenType.EqualTo:
				res = EvalEquals(left.ScrubObject, right.ScrubObject);
				break;
		}

		if (res == null)
		{
			res = new Result(
				new ScrubRuntimeError(
					$"Unable to do operation {bce.Operator} on {right.ScrubObject} and {left.ScrubObject}"));
		}

		return res;
	}

	private static Result? EvalEquals(ScrubObject left, ScrubObject right)
	{
		if (left.ScrubType == right.ScrubType)
		{
			///isn't it weird that i have to use Linq?
			return new Result(new ScrubObject(left.Data.SequenceEqual(right.Data)));
		}
		else
		{
			if (left.ScrubType == ScrubType.sDouble || right.ScrubType == ScrubType.sDouble)
			{
				var r = Math.Abs(left.ToNativeDouble() - right.ToNativeDouble()) < double.Epsilon;
				return new Result(new ScrubObject(r));
			}

			if (left.IsNumeric && right.IsNumeric)
			{
				var r = Math.Abs(left.ToNativeDouble() - right.ToNativeDouble()) < double.Epsilon;
				return new Result(new ScrubObject(r));
			}
			//0 == false? yes? no?
			// if (left.ScrubType == ScrubType.sBool || right.ScrubType == ScrubType.sBool)
			// {
			// 	return new Result(new ScrubObject(left.ToNativeBool() == right.ToNativeBool()));
			// }

			return new Result(null,
				new ScrubRuntimeError($"Unable to compare objects of type {left.ScrubType} and {right.ScrubType}"));
		}
	}
}