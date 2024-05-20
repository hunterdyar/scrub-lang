using System.Text;

namespace scrub_lang.Parser;

public class StringExpression : IExpression
{
	private readonly string _literal;

	public StringExpression(string literal)
	{
		_literal = literal;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('"');
		sb.Append(_literal);
		sb.Append('"');
	}
}