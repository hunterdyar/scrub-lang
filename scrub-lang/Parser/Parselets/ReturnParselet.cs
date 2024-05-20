using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class ReturnParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		//need to figure out how to handle return;
		//Should the tokenizer start adding endExpressions in when we have line breaks? I think so probably.
		
		if (parser.Peek(TokenType.EndExpression))
		{
			return new ReturnExpression(new NullExpression());
		}
		var right = parser.ParseExpression();
		return new ReturnExpression(right);
	}
}