using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

//Sub-lexer for all things that are valid combinations of "=,!,>,<"
//todo: This needs to get heck refactored.
public class EqualityTState(Tokenizer context) : TokenizerStateBase(context)
{
	private StringBuilder literal = new StringBuilder();
	private int firstCol = -1;
	public override void Consume(char c, int line, int col)
	{
		if (firstCol < 0)
		{
			firstCol = col;
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
				context.AddToken(new Token(TokenType.Assignment,s,line,firstCol));
			}else if (s == "==")
			{
				context.AddToken(new Token(TokenType.EqualTo,s,line,firstCol));
			}else if (s == "!=")
			{
				context.AddToken(new Token(TokenType.NotEquals, s, line, firstCol));
			}
			else if (s == ">")
			{
				context.AddToken(new Token(TokenType.GreaterThan, s, line, firstCol));
			}else if (s == "<")
			{
				context.AddToken(new Token(TokenType.LessThan, s, line, firstCol));
			}
			else if (s == ">=")
			{
				context.AddToken(new Token(TokenType.GreaterThanOrEqualTo, s, line, firstCol));
			}
			else if (s == "<=")
			{
				context.AddToken(new Token(TokenType.LessThanOrEqualTo, s, line, firstCol));
			}else if (s == "<<")
			{
				context.AddToken(new Token(TokenType.BitwiseLeftShift,s,line,firstCol));
			}else if (s == ">>")
			{
				context.AddToken(new Token(TokenType.BitwiseRightShift,s,line,firstCol));
			}
			else
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] == '!')
					{
						context.AddToken(new Token(TokenType.Bang, s, line, firstCol));
					}
					else
					{
						context.AddToken(new Token(TokenType.Unexpected, s, line, firstCol));
						break;
					}
				}
			}
			
			context.ExitState(this); //leave, but we haven't consumed anything yet, so we need to switch states.
			context.ConsumeNext(c, line, col);
		}
	}
}