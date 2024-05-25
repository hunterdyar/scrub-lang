using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public abstract class BinaryOperatorExpressionBase : IExpression
{
	public IExpression Left => _leftExpression;
	protected IExpression _leftExpression;
	public TokenType Operator => _operator;
	protected TokenType _operator;
	public IExpression Right => _rightExpression;
	protected IExpression _rightExpression;
	public Location Location { get; protected set; }


	public BinaryOperatorExpressionBase(IExpression leftExpression, TokenType op, IExpression rightExpression, Location location)
	{
		Location = location;
		_leftExpression = leftExpression;
		_operator = op;
		_rightExpression = rightExpression;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('(');
		_leftExpression.Print(sb);
		sb.Append(' ').Append(Token.OperatorToString(_operator)).Append(' ');
		_rightExpression.Print(sb);
		sb.Append(')');
	}
}