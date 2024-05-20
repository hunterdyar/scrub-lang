using System.Text;

namespace scrub_lang.Parser;

public class VariableDeclarationExpression : IExpression
{
	private IdentifierExpression _id;
	private IExpression _assignment;

	public VariableDeclarationExpression(IdentifierExpression id, IExpression assignment)
	{
		if (assignment == null)
		{
			_assignment = new NullExpression();
		}

		_id = id;
		_assignment = assignment;
	}
	public void Print(StringBuilder sb)
	{
		sb.Append(_id.Identifier);
		sb.Append(" = ");
		_assignment.Print(sb);
	}
}