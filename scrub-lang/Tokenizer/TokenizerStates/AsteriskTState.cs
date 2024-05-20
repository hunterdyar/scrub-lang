using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

public class AsteriskTState(Tokenizer context) : TokenizerStateBase(context)
{
	private int firstCol = -1;
	private char firstChar;

	public override void Consume(char c, int line, int col)
	{
		//todo: Generalize the "a or aa" states to share logic.
		if (firstCol < 0)
		{
			firstChar = c;
			firstCol = col;
			if (firstChar != '*')
			{
				//error: wrong state.
				Console.WriteLine("Error: Wrong Tokenizer State");
				context.AddToken(new Token(TokenType.Unexpected, c, line, col));
				context.ExitState(this);
				return;
			}

			return;
		}

		if (c != '*')
		{
			context.AddToken(new Token(TokenType.Multiply, firstChar, line, firstCol));
			context.ExitState(this);
			context.ConsumeNext(c, line, col);
			return;
		}

		if (c == '*')
		{
			context.AddToken(new Token(TokenType.PowerOf, firstChar.ToString() + c.ToString(), line, firstCol));
			context.ExitState(this);
			return;
		}
	}
}