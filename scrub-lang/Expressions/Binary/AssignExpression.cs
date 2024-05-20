using System.Text;

namespace scrub_lang.Parser;

public class AssignExpression : IExpression
{
	private readonly IdentifierExpression _identifier;
	private readonly IExpression _valueExpression;

	public AssignExpression(IdentifierExpression ident, IExpression valueExpr)
	{
		_identifier = ident;
		_valueExpression = valueExpr;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('(').Append(_identifier.Identifier).Append(" = ");
		_valueExpression.Print(sb);
		sb.Append(')');
	}
}