using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class TernaryParselet : IInfixParselet
{
	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		//left ? then : else
		var thenBranch = parser.ParseExpression();
		parser.Consume(TokenType.Colon);
		var elseBranch = parser.ParseExpression(BindingPower.Ternery - 1);

		return new  ConditionalExpression(left, thenBranch, elseBranch, token.Location);
	}

	public int GetBindingPower()
	{
		return BindingPower.Ternery;
	}
}