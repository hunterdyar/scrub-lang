using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class PostfixOperatorParselet : IInfixParselet
{
	private readonly int _bindingPower;

	public PostfixOperatorParselet(int bindingPower)
	{
		_bindingPower = bindingPower;
	}

	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		return new PostfixExpression(left, token.TokenType);
	}

	public int GetBindingPower()
	{
		return _bindingPower;
	}
}