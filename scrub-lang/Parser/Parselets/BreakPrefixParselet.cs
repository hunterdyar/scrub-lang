using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class BreakPrefixParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		return parser.ParseExpression();
	}
}