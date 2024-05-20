using System.Text;

namespace scrub_lang.Parser;

public class StringLiteralExpression : IExpression
{
	private readonly string _literal;

	public StringLiteralExpression(string literal)
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