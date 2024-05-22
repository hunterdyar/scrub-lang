using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class ArrayLookupParselet : IInfixParselet
{
	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		if(!(left is IdentifierExpression ie))
		{
			throw new ParseException($"Unable to Parse array lookup[] on {left}");
		}

		var index = parser.ParseExpression();
		parser.Consume(TokenType.CloseBracket);
		return new ArrayLookupExpression(ie, index);
	}

	public int GetBindingPower()
	{
		return BindingPower.ArraySubscript;
	}
}