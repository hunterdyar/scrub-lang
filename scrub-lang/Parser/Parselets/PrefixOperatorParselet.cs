using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class PrefixOperatorParselet : IPrefixParselet
{
	private readonly int _bindingPower;

	public PrefixOperatorParselet(int bindingPower)
	{
		_bindingPower = bindingPower;
	}

	public IExpression Parse(Parser parser, Token token)
	{
		var right = parser.ParseExpression(_bindingPower);
		return new PrefixExpression(token.TokenType, right);
	}

	public int GetBindingPower()
	{
		return _bindingPower;
	}
}