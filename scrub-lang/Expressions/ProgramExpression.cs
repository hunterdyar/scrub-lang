using System.Text;

namespace scrub_lang.Parser;

public class ProgramExpression : IExpression
{
	public List<IExpression> Expressions => _expressions;
	private List<IExpression> _expressions;

	public ProgramExpression(List<IExpression> expressions)
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