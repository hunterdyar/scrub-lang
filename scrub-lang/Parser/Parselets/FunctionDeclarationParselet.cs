using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class FunctionDeclarationParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		//consume func
		var idtoken = parser.Consume(TokenType.Identifier);
		var id = new IdentifierExpression(idtoken.Literal);
		var args = new List<IdentifierExpression>();

		parser.Consume(TokenType.OpenParen);
		if (parser.Peek(TokenType.Identifier))
		{
			do
			{
				args.Add(new IdentifierExpression(parser.Consume(TokenType.Identifier).Literal));
			} while (parser.Match(TokenType.Comma));
		}

		parser.Consume(TokenType.CloseParen);
		var exp = parser.ParseExpression();
		return new FunctionDeclarationExpression(id, args, exp);
		
		
		//consume function signature
		//consume expression block
	}
}