using System.Text;

namespace scrub_lang.Parser;

public class AssignExpression : IExpression
{
	public IdentifierExpression Identifier => _identifier;
	private readonly IdentifierExpression _identifier;
	public IExpression Value => _valueExpression;
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