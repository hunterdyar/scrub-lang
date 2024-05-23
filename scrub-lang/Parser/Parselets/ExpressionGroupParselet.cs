using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class ExpressionGroupParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		List<IExpression> expressions = new List<IExpression>();
		while (!parser.Peek(TokenType.EndExpressionBlock) && !parser.Peek(TokenType.EOF))
		{
			var e = parser.ParseExpression(0);
			expressions.Add(e);
		}

		parser.Consume(TokenType.EndExpressionBlock);

		return new ExpressionGroupExpression(expressions);
	}
}