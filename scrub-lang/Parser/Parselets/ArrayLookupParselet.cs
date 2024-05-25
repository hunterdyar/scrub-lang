using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class ArrayLookupParselet : IInfixParselet
{
	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		var index = parser.ParseExpression();
		parser.Consume(TokenType.CloseBracket);
		return new IndexExpression(left, index, left.Location);//location of a[0] is a, not [.
	}

	public int GetBindingPower()
	{
		return BindingPower.ArraySubscript;
	}
}