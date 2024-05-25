using System.Text;

namespace scrub_lang.Parser;

public class NullExpression : IExpression
{
	public Location Location { get; }

	public NullExpression(Location location)
	{
		Location = location;
	}
	public void Print(StringBuilder sb)
	{
		sb.Append("Null");
	}
}