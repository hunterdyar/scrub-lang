using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class VariableDeclarationParselet : IPrefixParselet
{
	public IExpression Parse(Parser parser, Token token)
	{
		//var a = 3;
		//var a;
		var id = parser.Consume(TokenType.Identifier);
		var ide = new IdentifierExpression(id.Literal);
		//var a ;
		if (parser.Peek(TokenType.EndExpression))
		{
			parser.Match(TokenType.EndExpression);
			return new VariableDeclarationExpression(ide, new NullExpression());
		}
		//var a = 3;
		if (parser.Match(TokenType.Assignment))
		{
			var right = parser.ParseExpression();
			return new VariableDeclarationExpression(ide, right);
		}

		throw new ParseException("Unable to parse declaration");
		
	}
}