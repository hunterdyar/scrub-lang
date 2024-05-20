using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class BinaryBitwiseOperator : BinaryOperatorExpression
{
	public BinaryBitwiseOperator(IExpression leftExpression, TokenType op, IExpression rightExpression) : base(leftExpression, op, rightExpression)
	{
		if (!IsBinaryBitwiseOperator(op))
		{
			throw new ParseException($"{op} is not a Bitwise Operator!");
		}
	}

	public static bool IsBinaryBitwiseOperator(TokenType op)
	{
		return op == TokenType.BitwiseAnd || op == TokenType.BitwiseOr || op == TokenType.BitwiseLeftShift ||
		       op == TokenType.BitwiseRightShift || op == TokenType.BitwiseOr;
	}
}