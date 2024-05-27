using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class BinaryBitwiseExpression : BinaryOperatorExpressionBase
{
	public BinaryBitwiseExpression(IExpression leftExpression, TokenType op, IExpression rightExpression, Location location) : base(leftExpression, op, rightExpression, location)
	{
		Location = location;
		_leftExpression = leftExpression;
		_operator = op;
		_rightExpression = rightExpression;
		
		if (!IsBinaryBitwiseOperator(op))
		{
			throw new ParseException($"{op} is not a Bitwise Operator!");
		}
	}

	public static bool IsBinaryBitwiseOperator(TokenType op)
	{
		return op == TokenType.BitwiseAnd
		       || op == TokenType.BitwiseOr
		       || op == TokenType.BitwiseLeftShift
		       || op == TokenType.BitwiseRightShift 
		       || op == TokenType.BitwiseXOR;
	}
}