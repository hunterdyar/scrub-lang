using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class FunctionDeclarationParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		//consume func(){} and func a(){}
		IdentifierExpression id = null;
		Location idLocation;
		if (parser.Peek(TokenType.Identifier))
		{
			var idtoken = parser.Consume(TokenType.Identifier);
			idLocation = idtoken.Location;
			id = new IdentifierExpression(idtoken.Literal, idtoken.Location);
		}

		var args = new List<IdentifierExpression>();

		parser.Consume(TokenType.OpenParen);
		if (parser.Peek(TokenType.Identifier))
		{
			do
			{
				var idtok = parser.Consume(TokenType.Identifier);
				args.Add(new IdentifierExpression(idtok.Literal, idtok.Location));
			} while (parser.Match(TokenType.Comma));
		}

		parser.Consume(TokenType.CloseParen);
		var exp = parser.ParseExpression();

		if (id == null)
		{
			return new FunctionLiteralExpression(args, exp, token.Location);
		}
		else
		{
			return new FunctionDeclarationExpression(id, new FunctionLiteralExpression(args, exp, id.Location, id.Identifier), token.Location);// func identity(){}
		}

		//consume function signature
		//consume expression block
	}
}