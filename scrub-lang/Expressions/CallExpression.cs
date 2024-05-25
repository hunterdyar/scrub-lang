using System.Text;

namespace scrub_lang.Parser;
/// <summary>
/// Function call like a(b,d,d);
/// </summary>
public class CallExpression : IExpression
{
	public IExpression Expression => _functionExpression;
	private IExpression _functionExpression;
	public IExpression[] Args => _argumentExprs;
	private IExpression[] _argumentExprs;
	public Location Location { get; }

	public CallExpression(IExpression functionExpression, List<IExpression> argumentExpressions, Location location)
	{
		Location = location;
		_functionExpression = functionExpression;
		_argumentExprs = argumentExpressions.ToArray();
	}

	public void Print(StringBuilder sb)
	{
		_functionExpression.Print(sb);
		sb.Append('(');
		for (int i = 0; i < _argumentExprs.Length; i++)
		{
			if (i > 0) sb.Append(", ");
			_argumentExprs[i].Print(sb);
		}

		sb.Append(")");
	}
}