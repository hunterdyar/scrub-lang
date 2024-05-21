using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class IncrementExpression : PostfixExpressionBase
{
	public IncrementExpression(IExpression left, TokenType op) : base(left, op)
	{
	}
}