using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class BoolLiteralExpression : IExpression
{
	public bool Literal => _literalValue;
	private bool _literalValue;
	public Location Location { get; }
	public BoolLiteralExpression(TokenType keyword, Location location)
	{
		Location = location;
		if (keyword == TokenType.TrueKeyword)
		{
			_literalValue = true;
		}else if (keyword == TokenType.FalseKeyword)
		{
			_literalValue = false;
		}
		else
		{
			throw new ParseException($"Unable to turn {keyword} into litearl for true or false");
		}
	}

	public void Print(StringBuilder sb)
	{
		sb.Append(_literalValue ? "true" : "false");
	}
}