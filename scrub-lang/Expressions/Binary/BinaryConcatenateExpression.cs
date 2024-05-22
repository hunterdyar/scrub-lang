using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class BinaryConcatenateExpression : BinaryOperatorExpressionBase
{
	public BinaryConcatenateExpression(IExpression leftExpression, TokenType op, IExpression rightExpression) : base(leftExpression, op, rightExpression)
	{
		_leftExpression = leftExpression;
		_operator = op;
		_rightExpression = rightExpression;

		if (!IsConcatenateOperator(op))
		{
			throw new ParseException($"{op} is not a concatenate operator.");
		}
	}

	public static bool IsConcatenateOperator(TokenType op)
	{
		return op == TokenType.IncrementConcatenate;
	}
}