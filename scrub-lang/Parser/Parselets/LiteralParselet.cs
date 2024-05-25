using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class LiteralParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		if (token.TokenType == TokenType.NumberLiteral)
		{
			return new NumberLiteralExpression(token.Literal, token.Location);
		}

		if (token.TokenType == TokenType.String)
		{
			return new StringLiteralExpression(token.Literal, token.Location);
		}
		throw new ParseException("uh oh!");
	}
}