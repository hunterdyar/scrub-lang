using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class BreakInfixParselet : IInfixParselet
{
	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		return left;
	}

	public int GetBindingPower()
	{
		return BindingPower.Break;
	}
}