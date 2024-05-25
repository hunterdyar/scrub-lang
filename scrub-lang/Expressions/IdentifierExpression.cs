using System.Text;

namespace scrub_lang.Parser;

public class IdentifierExpression : IExpression
{
	public string Identifier;
	public Location Location { get; }

	public IdentifierExpression(string identifier, Location location)
	{
		Location = location;
		Identifier = identifier;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append(Identifier);
	}
}