using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class ArrayLiteralParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		//consume the first [
		List<IExpression> values = new List<IExpression>();
		while (!parser.Match(TokenType.Comma) && !parser.Match(TokenType.CloseBracket))// we could never matcch both, so we can use the consuming (match) here.
		{
			var v = parser.ParseExpression();
			values.Add(v);
		}

		//parser.Consume(TokenType.CloseBracket);
		return new ArrayLiteralExpression(values);
	}
}