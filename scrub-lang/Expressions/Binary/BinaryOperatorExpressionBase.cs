using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public abstract class BinaryOperatorExpressionBase : IExpression
{
	private IExpression _leftExpression;
	private TokenType _operator;
	private IExpression _rightExpression;

	public BinaryOperatorExpressionBase(IExpression leftExpression, TokenType op, IExpression rightExpression)
	{
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