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
			if (right is FunctionLiteralExpression rf)
			{
				//a = func(){] should work like func a(){}
				rf.Name = name.Identifier;
			}
			return new AssignExpression(name, right, token.Location);
		}else if (left is IndexExpression index)
		{
			//todo: assign index expressions. A built-in called set that it replaces itself with?
		}
		throw new ParseException($"The left-hand side of an assignment ({token.Location}) must be an identifier, at {left.Location}");
	}

	public int GetBindingPower()
	{
		return BindingPower.Assignment;
	}
}