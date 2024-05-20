using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class IdentifierParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		switch (token.TokenType)
		{
			case TokenType.TrueKeyword:
				return new BoolLiteralExpression(token.TokenType);
			case TokenType.FalseKeyword:
				return new BoolLiteralExpression(token.TokenType);
			//null
		}
		return new IdentifierExpression(token.Literal);
	}
}