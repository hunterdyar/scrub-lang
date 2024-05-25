using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

//Sub-lexer for all things that are valid combinations of "=,!,>,<"
//todo: This needs to get heck refactored.
public class EqualityTState(Tokenizer context) : TokenizerStateBase(context)
{
	private StringBuilder literal = new StringBuilder();
	private int firstCol = -1;
	public override void Consume(char c, Location loc)
	{
		if (firstCol < 0)
		{
			firstCol = loc.Column;
		}
		
		//we keep adding these things until we hit something different.
		if (c == '=' || c == '>' || c == '<' || c == '!')
		{
			literal.Append(c);
			//do nothing, don't leave the state, wait for some other character to break us out.
		}
		else
		{
			var s = literal.ToString();
			if (s == "=")
			{
				context.AddToken(new Token(TokenType.Assignment,s,loc));
			}else if (s == "==")
			{
				context.AddToken(new Token(TokenType.EqualTo,s,loc.Line,firstCol));
			}else if (s == "!=")
			{
				context.AddToken(new Token(TokenType.NotEquals, s, loc.Line, firstCol));
			}
			else if (s == ">")
			{
				context.AddToken(new Token(TokenType.GreaterThan, s, loc.Line, firstCol));
			}else if (s == "<")
			{
				context.AddToken(new Token(TokenType.LessThan, s, loc.Line, firstCol));
			}
			else if (s == ">=")
			{
				context.AddToken(new Token(TokenType.GreaterThanOrEqualTo, s, loc.Line, firstCol));
			}
			else if (s == "<=")
			{
				context.AddToken(new Token(TokenType.LessThanOrEqualTo, s, loc.Line, firstCol));
			}else if (s == "<<")
			{
				context.AddToken(new Token(TokenType.BitwiseLeftShift,s, loc.Line,firstCol));
			}else if (s == ">>")
			{
				context.AddToken(new Token(TokenType.BitwiseRightShift,s, loc.Line,firstCol));
			}
			else
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] == '!')
					{
						context.AddToken(new Token(TokenType.Bang, s, loc.Line, firstCol));
					}
					else
					{
						context.AddToken(new Token(TokenType.Unexpected, s, loc.Line, firstCol));
						break;
					}
				}
			}
			
			context.ExitState(this); //leave, but we haven't consumed anything yet, so we need to switch states.
			context.ConsumeNext(c, loc);
		}
	}
}