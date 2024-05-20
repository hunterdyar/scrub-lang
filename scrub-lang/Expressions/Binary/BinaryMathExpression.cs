using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class BinaryMathExpression : BinaryOperatorExpressionBase
{
	public IExpression Left => _leftExpression;
	private IExpression _leftExpression;
	public TokenType Operator => _operator;
	private TokenType _operator;

	public IExpression Right => _rightExpression;
	private IExpression _rightExpression;

	public BinaryMathExpression(IExpression leftExpression, TokenType op, IExpression rightExpression) : base(leftExpression,op,rightExpression)
	{
		_leftExpression = leftExpression;
		_operator = op;
		_rightExpression = rightExpression;
		
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