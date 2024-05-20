using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public interface IInfixParselet
{
	IExpression Parse(Parser parser, IExpression left, Token token);
	int GetBindingPower();
}