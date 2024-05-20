using System.Text;

namespace scrub_lang.Parser;
/// <summary>
/// Function call like a(b,d,d);
/// </summary>
public class CallExpression : IExpression
{
	private IExpression _functionExpression;
	private List<IExpression> _argumentExprs;

	public CallExpression(IExpression functionExpression, List<IExpression> argumentExpressions)
	{
		_functionExpression = functionExpression;
		_argumentExprs = argumentExpressions;
	}

	public void Print(StringBuilder sb)
	{
		_functionExpression.Print(sb);
		sb.Append('(');
		for (int i = 0; i < _argumentExprs.Count; i++)
		{
			if (i > 0) sb.Append(", ");
			_argumentExprs[i].Print(sb);
		}

		sb.Append(");");
	}
}