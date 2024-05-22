using System.Text;

namespace scrub_lang.Parser;

public class IndexExpression : IExpression 
{
	public IExpression Left => _left;
	private IExpression _left;
	public IExpression Index => _index;
	private IExpression _index;

	public IndexExpression(IExpression left, IExpression index)
	{
		_left = left;
		_index = index;
	}

	public void Print(StringBuilder sb)
	{
		_left.Print(sb);
		sb.Append('[');
		_index.Print(sb);
		sb.Append(']');
	}
}