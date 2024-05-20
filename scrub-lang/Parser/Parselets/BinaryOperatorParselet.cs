using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class BinaryOperatorParselet : IInfixParselet
{
	private readonly int _bindingPower;
	private readonly bool _isRight;

	public BinaryOperatorParselet(int bindingPower, bool isRight)
	{
		_bindingPower = bindingPower;
		_isRight = isRight;
	}

	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		//right hand side slightly lower so that ^binary and 2^3 work correctly.
		IExpression right = parser.ParseExpression(_bindingPower - (_isRight ? 1 : 0));
		return new BinaryOperatorExpression(left, token.TokenType, right);
	}

	public int GetBindingPower()
	{
		return _bindingPower;
	}
}