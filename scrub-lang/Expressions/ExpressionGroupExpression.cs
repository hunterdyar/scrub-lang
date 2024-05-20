using System.Text;

namespace scrub_lang.Parser;

public class ExpressionGroupExpression : IExpression
{
	public List<IExpression> Expressions => _expressions;
	private List<IExpression> _expressions;

	public ExpressionGroupExpression(List<IExpression> expressions)
	{
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