using System.Text;

namespace scrub_lang.Parser;

public class NullExpression : IExpression
{
	public void Print(StringBuilder sb)
	{
		sb.Append("Null");
	}
}