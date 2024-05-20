using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class IdentifierParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		return new IdentifierExpression(token.Literal);
	}
}