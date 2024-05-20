using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

public class IncrementTState(Tokenizer context) : TokenizerStateBase(context)
{
	private char firstChar;
	private char secondChar;
	private int firstCol = -1;
	public override void Consume(char c, int line, int col)
	{
		if (firstCol < 0)
		{
			firstCol = col;
			firstChar = c;
			if (firstChar != '+' && firstChar != '-')
			{
				//error: wrong state.
				Console.WriteLine("Error: Wrong Tokenizer State");
				context.AddToken(new Token(TokenType.Unexpected, c, line, col));
				context.ExitState(this);
				return;
			}

			return;
		}
		else
		{
			secondChar = c;
		}
		//options are: 
		//+
		//++
		//-
		//--

		if (firstChar == '+' && secondChar == '+')
		{
			context.AddToken(new Token(TokenType.Increment, firstChar.ToString() + secondChar.ToString(), line, firstCol));
			context.ExitState(this);
			return;
		}

		if (firstChar == '-' && secondChar == '-')
		{
			context.AddToken(new Token(TokenType.Decrement, firstChar.ToString() + secondChar.ToString(), line, firstCol));
			context.ExitState(this);
			return;
		}

		if (firstChar == '+' && secondChar != '+')
		{
			context.AddToken(new Token(TokenType.Plus, firstChar.ToString(), line, firstCol));
			context.ExitState(this);
			context.ConsumeNext(c, line, col);
			return;
		}

		if (firstChar == '-' && secondChar != '-')
		{
			context.AddToken(new Token(TokenType.Minus, firstChar.ToString(), line, firstCol));
			context.ExitState(this);
			context.ConsumeNext(c, line, col);
			return;
		}
		
	}
}