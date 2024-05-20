using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public interface IPrefixParselet
{
	IExpression Parse(Parser parser, Token token);
}