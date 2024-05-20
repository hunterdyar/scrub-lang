using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class PrefixExpression : IExpression
{
	private TokenType _operator;
	private IExpression _rightExpr;

	public PrefixExpression(TokenType op, IExpression rightExpr)
	{
		_operator = op;
		this._rightExpr = rightExpr;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('(').Append(Token.OperatorToString(_operator));
		_rightExpr.Print(sb);
		sb.Append(')');
	}
	
}