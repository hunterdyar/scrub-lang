using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

//a = b = c is parsed as a = (b = c)
public class AssignParselet : IInfixParselet
{
	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		var right = parser.ParseExpression(BindingPower.Assignment - 1);
		if (left is IdentifierExpression name)
		{
			return new AssignExpression(name, right);
		}else if (left is IndexExpression index)
		{
			//todo: assign index expressions. A built-in called set that it replaces itself with?
		}
		throw new ParseException("The left-hand side of an assignment must be an identifier.");
	}

	public int GetBindingPower()
	{
		return BindingPower.Assignment;
	}
}