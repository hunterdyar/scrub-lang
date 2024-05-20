using System.Text;

namespace scrub_lang.Parser;

public class Program : IExpression
{
	private List<IExpression> _expressions;

	public Program(List<IExpression> expressions)
	{
		_expressions = expressions;
	}

	public void Print(StringBuilder sb)
	{
		for (var i = 0; i < _expressions.Count; i++)
		{
			var e = _expressions[i];
			e.Print(sb);
			if (i < _expressions.Count - 1)
			{
				sb.Append("\n");
			}
		}
	}
}