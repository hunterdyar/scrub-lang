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
		if (token.TokenType == TokenType.IncrementConcatenate)
		{
			return new IncrementExpression(left, token.TokenType, token.Location);
		}else if (token.TokenType == TokenType.Decrement)
		{
			return new DecrementExpression(left, token.TokenType, token.Location);
		}

		throw new ParseException($"Unable to parse Postfix operator {token.TokenType}");
	}

	public int GetBindingPower()
	{
		return _bindingPower;
	}
}