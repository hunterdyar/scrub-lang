using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class BinaryConditionalExpression : BinaryOperatorExpressionBase
{
	public BinaryConditionalExpression(IExpression leftExpression, TokenType op, IExpression rightExpression, Location location) : base(leftExpression, op, rightExpression, location)
	{
		if (!IsBinaryConditionalOperator(op))
		{
			throw new ParseException($"{op} is not a conditional operator");
		}
	}

	public static bool IsBinaryConditionalOperator(TokenType op)
	{
		//SHould we split numeric (greater, etc) from pure conditional (and, or)?
		//We should... but where does == go? Uh oh!
		//hmm i kind of like having "=int=", "=bool=", etc. it's terrible but i like it! how else would an untyped language work? lol
		return op == TokenType.EqualTo 
		       || op == TokenType.NotEquals 
		       || op == TokenType.GreaterThan
		       || op == TokenType.LessThan
		       || op == TokenType.GreaterThanOrEqualTo
		       || op == TokenType.And
		       || op == TokenType.Or
		       || op == TokenType.LessThanOrEqualTo;
	}
}