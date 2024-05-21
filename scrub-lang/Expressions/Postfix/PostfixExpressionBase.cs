using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public abstract class PostfixExpressionBase : IExpression
{
	public IExpression Left => _leftExpr;
	private IExpression _leftExpr;
	public TokenType Operator => _operator;
	private TokenType _operator;

	public PostfixExpressionBase(IExpression left, TokenType op)
	{
		_leftExpr = left;
		_operator = op;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('(');
		_leftExpr.Print(sb);
		sb.Append(Token.OperatorToString(_operator)).Append(')');
	}
}