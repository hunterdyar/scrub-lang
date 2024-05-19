using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

public class NumberTState(Tokenizer context) : TokenizerStateBase(context)
{
	private StringBuilder numberLiteral = new();

	public override void Consume(char c, int line, int col)
	{
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
				context.AddToken(new Token(TokenType.NumberLiteral, lit, line, col));
				context.ExitState(this);
				context.ConsumeNext(c, line, col);

			}
			else
			{
				var lit = numberLiteral.ToString();
				context.AddToken(new Token(TokenType.NumberLiteral, lit, line, col));
				context.ExitState(this);
				context.ConsumeNext(c,line,col);
			}
		}
	}
}