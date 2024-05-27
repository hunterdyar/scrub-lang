using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;
//todo: allow hex, octal, and binary literal values.
public class NumberTState(Tokenizer context) : TokenizerStateBase(context)
{
	private StringBuilder numberLiteral = new();

	public override void Consume(char c, Location loc)
	{
		//first character has to be a digit, but then it can be a b (0b0001) or x or o; or a d (32d is 32 forced type to double)
		//we also could suppport _'s and so on, like some ways of writing numbers. i'm more interested in supporting binary, hex, octal tho.
		
		if (char.IsDigit(c) || char.IsNumber(c) || c == '.')
		{
			numberLiteral.Append(c);
		}
		else
		{
			string s = numberLiteral.ToString();
			if (s.Contains("."))
			{
				var lit = numberLiteral.ToString();
				context.AddToken(new Token(TokenType.NumberLiteral, lit, loc));
				context.ExitState(this);
				context.ConsumeNext(c, loc);
			}
			else
			{
				var lit = numberLiteral.ToString();
				context.AddToken(new Token(TokenType.NumberLiteral, lit, loc));
				context.ExitState(this);
				context.ConsumeNext(c,loc);
			}
		}
	}
}