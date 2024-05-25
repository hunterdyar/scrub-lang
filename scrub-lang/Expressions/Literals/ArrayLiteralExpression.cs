using System.Text;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.Parser;

//[a,b,c]
public class ArrayLiteralExpression : IExpression
{
	public List<IExpression> Values => _values;
	private readonly List<IExpression> _values;
	public Location Location { get; }

	public ArrayLiteralExpression(List<IExpression> values, Location location)
	{
		Location = location;
		this._values = values;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('[');
		for (int i = 0; i < Values.Count; i++)
		{
			Values[i].Print(sb);
			if (i < _values.Count - 1)
			{
				sb.Append(", ");
			}
		}

		sb.Append(']');
	}
}