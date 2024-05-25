using System.Text;

namespace scrub_lang.Parser;

public class ExpressionGroupExpression : IExpression
{
	public  IExpression[] Expressions => _expressions;
	private IExpression[] _expressions;
	public Location Location { get; }

	public ExpressionGroupExpression(IExpression[] expressions, Location location)
	{
		Location = location;
		_expressions = expressions;
	}


	public void Print(StringBuilder sb)
	{
		sb.Append("{\n");
		foreach (var e in _expressions)
		{
			e.Print(sb);
			sb.Append('\n');
		}

		sb.Append('}');
	}
}