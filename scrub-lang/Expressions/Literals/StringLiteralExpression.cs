using System.Text;
using String = scrub_lang.Objects.String;

namespace scrub_lang.Parser;

public class StringLiteralExpression : IExpression
{
	public string Literal => _literal;
	private readonly string _literal;
	public Location Location { get; }
	public StringLiteralExpression(string literal, Location location)
	{
		Location = location;
		_literal = literal;
	}

	public String GetScrubObject()
	{
		return new String(_literal);
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('"');
		sb.Append(_literal);
		sb.Append('"');
	}
}