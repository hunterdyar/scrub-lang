using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class InlineGroupParselet : IPrefixParselet
{
	//parses parenthesis uses to group an expression, like "(b+c)"
	public IExpression Parse(Parser parser, Token token)
	{
		var expression = parser.ParseExpression();
		parser.Consume(TokenType.CloseParen);
		return expression;
	}
}