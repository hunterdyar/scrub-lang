using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

public class AsteriskTState(Tokenizer context) : TokenizerStateBase(context)
{
	private int firstCol = -1;
	private char firstChar;

	public override void Consume(char c, Location loc)
	{
		//todo: Generalize the "a or aa" states to share logic.
		if (firstCol < 0)
		{
			firstChar = c;
			firstCol = loc.Column;
			if (firstChar != '*')
			{
				//error: wrong state.
				Console.WriteLine("Error: Wrong Tokenizer State");
				context.AddToken(new Token(TokenType.Unexpected, c,loc));
				context.ExitState(this);
				return;
			}

			return;
		}

		if (c != '*')
		{
			context.AddToken(new Token(TokenType.Multiply, firstChar,loc));
			context.ExitState(this);
			context.ConsumeNext(c, loc);
			return;
		}

		if (c == '*')
		{
			context.AddToken(new Token(TokenType.PowerOf, firstChar.ToString() + c.ToString(), loc));
			context.ExitState(this);
			return;
		}
	}
}