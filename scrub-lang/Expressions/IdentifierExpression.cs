using System.Text;

namespace scrub_lang.Parser;

public class IdentifierExpression : IExpression
{
	public string Identifier;
	
	public IdentifierExpression(string identifier)
	{
		Identifier = identifier;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append(Identifier);
	}
}