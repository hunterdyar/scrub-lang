using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class IdentifierParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		switch (token.TokenType)
		{
			case TokenType.TrueKeyword:
				return new BoolLiteralExpression(token.TokenType, token.Location);
			case TokenType.FalseKeyword:
				return new BoolLiteralExpression(token.TokenType, token.Location);
			case TokenType.NullKeyword:
				return new NullExpression(token.Location);
			//null
		}
		return new IdentifierExpression(token.Literal, token.Location);
	}
}