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

		if (token.TokenType == TokenType.HexLiteral)
		{
			return new NumberLiteralExpression(token.Literal, token.Location, 16);
		}

		if (token.TokenType == TokenType.BinaryLiteral)
		{
			return new NumberLiteralExpression(token.Literal, token.Location, 2);
		}

		if (token.TokenType == TokenType.OctalLiteral)
		{
			return new NumberLiteralExpression(token.Literal, token.Location, 8);
		}
		throw new ParseException("uh oh!");
	}
}