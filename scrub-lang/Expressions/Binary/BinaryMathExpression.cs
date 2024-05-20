using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class BinaryMathExpression : BinaryOperatorExpressionBase
{
	private IExpression _leftExpression;
	private TokenType _operator;
	private IExpression _rightExpression;

	public BinaryMathExpression(IExpression leftExpression, TokenType op, IExpression rightExpression) : base(
		leftExpression, op, rightExpression)
	{
		if (!IsBinaryMathOperator(op))
		{
			throw new ParseException($"{op} is not a binary math operator.");
		}
	}
	public static bool IsBinaryMathOperator(TokenType op)
	{
		return op == TokenType.Plus 
		       || op == TokenType.Multiply 
		       || op == TokenType.Minus
		       || op == TokenType.Division 
		       || op == TokenType.PowerOf
		       || op == TokenType.Modulo;
	}
}