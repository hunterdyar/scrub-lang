using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class DecrementExpression : PostfixExpressionBase
{
	public DecrementExpression(IExpression left, TokenType op, Location location) : base(left, op, location)
	{
	}
}