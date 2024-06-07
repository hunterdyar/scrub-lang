using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class ImportParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		//need to figure out how to handle return;
		//Should the tokenizer start adding endExpressions in when we have line breaks? I think so probably.
		//I also want the rust syntax of whatever the last value in an expression being what that block evaluates to.
		//If this is the case, then return is always a single keyword.

		if (parser.Peek(TokenType.Break))
		{
			return new ImportExpression(new NullExpression(token.Location), token.Location);
		}

		var right = parser.ParseExpression();
		return new ImportExpression(right, token.Location);
	}
}