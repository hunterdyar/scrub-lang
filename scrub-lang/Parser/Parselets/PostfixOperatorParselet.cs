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
			//todo: more robust a++ vs. 'a++b' parsing without lookahead. i think the conditions for binary are more knowable? identifier, literal, open brackets, open function... hmmm
			if (parser.Peek(TokenType.Break) || parser.Peek(TokenType.CloseParen) || parser.Peek(TokenType.CloseBracket) || parser.Peek(TokenType.Comma) || parser.Peek(TokenType.Colon) || parser.Peek(TokenType.Question))
			{
				return new IncrementExpression(left, token.TokenType, token.Location);
			}
			else
			{
				return new BinaryConcatenateExpression(left, token.TokenType, parser.ParseExpression(), token.Location);
			}
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