using System.Text;

namespace scrub_lang.Parser;

public interface IExpression
{
	public Location Location { get; }
	void Print(StringBuilder sb);
}