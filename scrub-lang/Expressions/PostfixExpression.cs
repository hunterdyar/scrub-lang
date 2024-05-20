using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class PostfixExpression : IExpression
{
	private IExpression _leftExpr;
	private TokenType _operator;

	public PostfixExpression(IExpression left, TokenType op)
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