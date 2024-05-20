using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

//a = b = c is parsed as a = (b = c)
public class AssignParselet : IInfixParselet
{
	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		var right = parser.ParseExpression(BindingPower.Assignment - 1);

		if (!(left is IdentifierExpression))
			throw new ParseException("The left-hand side of an assignment must be an identifier.");

		var name = ((IdentifierExpression)left);
		return new AssignExpression(name, right);
	}

	public int GetBindingPower()
	{
		return BindingPower.Assignment;
	}
}