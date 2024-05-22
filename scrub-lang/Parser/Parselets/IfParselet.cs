using System.Collections.Concurrent;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class IfParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		parser.Consume(TokenType.OpenParen);
		var conditionalExpression = parser.ParseExpression();
		parser.Consume(TokenType.CloseParen);
		//every bone in my body says to parse this as a block and don't allow inlines.
		//but everything is an expression, so it's actually fine!
		var thenBranch = parser.ParseExpression();
		 IExpression elseBranch = new NullExpression();
		if (parser.Peek(TokenType.ElseKeyword))
		{
			parser.Consume(TokenType.ElseKeyword);
			elseBranch = parser.ParseExpression();
		}

		return new ConditionalExpression(conditionalExpression, thenBranch, elseBranch);
	}
}