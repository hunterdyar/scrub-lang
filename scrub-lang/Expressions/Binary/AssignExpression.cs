using System.Text;

namespace scrub_lang.Parser;

public class AssignExpression : IExpression
{
	public IdentifierExpression Assignee => _assignee;
	private readonly IdentifierExpression _assignee;
	public IExpression Value => _valueExpression;
	private readonly IExpression _valueExpression;
	public Location Location { get; }

	public AssignExpression(IdentifierExpression assignee, IExpression valueExpr, Location location)
	{
		Location = location;
		_assignee = assignee;
		_valueExpression = valueExpr;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('(').Append(_assignee.Identifier).Append(" = ");
		_valueExpression.Print(sb);
		sb.Append(')');
	}
}