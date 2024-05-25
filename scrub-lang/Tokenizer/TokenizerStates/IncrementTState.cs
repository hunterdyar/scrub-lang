using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

public class IncrementTState(Tokenizer context) : TokenizerStateBase(context)
{
	private char firstChar;
	private char secondChar;
	private int firstCol = -1;
	public override void Consume(char c, Location loc)
	{
		if (firstCol < 0)
		{
			firstCol = loc.Column;
			firstChar = c;
			if (firstChar != '+' && firstChar != '-')
			{
				//error: wrong state.
				Console.WriteLine("Error: Wrong Tokenizer State");
				context.AddToken(new Token(TokenType.Unexpected, c, loc));
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
			context.AddToken(new Token(TokenType.IncrementConcatenate, firstChar.ToString() + secondChar.ToString(), loc.Line, firstCol));
			context.ExitState(this);
			return;
		}

		if (firstChar == '-' && secondChar == '-')
		{
			context.AddToken(new Token(TokenType.Decrement, firstChar.ToString() + secondChar.ToString(), loc.Line, firstCol));
			context.ExitState(this);
			return;
		}

		if (firstChar == '+' && secondChar != '+')
		{
			context.AddToken(new Token(TokenType.Plus, firstChar.ToString(), loc.Line, firstCol));
			context.ExitState(this);
			context.ConsumeNext(c, loc);
			return;
		}

		if (firstChar == '-' && secondChar != '-')
		{
			context.AddToken(new Token(TokenType.Minus, firstChar.ToString(), loc.Line, firstCol));
			context.ExitState(this);
			context.ConsumeNext(c, loc);
			return;
		}
	}
}