using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class BoolLiteralExpression : IExpression
{
	private bool literalValue;

	public BoolLiteralExpression(TokenType keyword)
	{
		if (keyword == TokenType.TrueKeyword)
		{
			literalValue = true;
		}else if (keyword == TokenType.FalseKeyword)
		{
			literalValue = false;
		}
		else
		{
			throw new ParseException($"Unable to turn {keyword} into litearl for true or false");
		}
	}

	public void Print(StringBuilder sb)
	{
		sb.Append(literalValue ? "true" : "false");
	}
}