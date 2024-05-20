using System.Text;

namespace scrub_lang.Parser;

public class AssignExpression : IExpression
{
	private readonly string _identifier;
	private readonly IExpression _valueExpression;

	public AssignExpression(string ident, IExpression valueExpr)
	{
		_identifier = ident;
		_valueExpression = valueExpr;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('(').Append(_identifier).Append(" = ");
		_valueExpression.Print(sb);
		sb.Append(')');
	}
}