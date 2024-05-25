using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

/// <summary>
/// Handles '&' and '|' characters.
/// </summary>
public class ComparisonTState(Tokenizer context) : TokenizerStateBase(context)
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
			if (firstChar != '&' && firstChar != '|')
			{
				//error: wrong state.
				Console.WriteLine("Error: Wrong Tokenizer State");
				context.AddToken(new Token(TokenType.Unexpected,c,loc));
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
		//&
		//&&
		//|
		//||

		if (firstChar == '&' && secondChar == '&')
		{
			context.AddToken(new Token(TokenType.And, firstChar.ToString() + secondChar.ToString(), loc));
			context.ExitState(this);
			return;
		}

		if (firstChar == '|' && secondChar == '|')
		{
			context.AddToken(new Token(TokenType.Or, firstChar.ToString() + secondChar.ToString(), loc));
			context.ExitState(this);
			return;
		}

		if (firstChar == '&' && secondChar != '&')
		{
			context.AddToken(new Token(TokenType.BitwiseAnd, firstChar.ToString(), loc.Line,firstCol)); // location of && is the first &
			context.ExitState(this);
			context.ConsumeNext(c, loc);
			return;
		}
		
		if (firstChar == '|' && secondChar != '|')
		{
			context.AddToken(new Token(TokenType.BitwiseOr, firstChar.ToString(), loc.Line, firstCol)); // location of c is the first c
			context.ExitState(this);
			context.ConsumeNext(c,loc);
			return;
		}
	}
}