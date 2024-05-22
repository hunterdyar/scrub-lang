using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

public class StringTState(Tokenizer context) : TokenizerStateBase(context)
{
	private StringBuilder _builder = new StringBuilder();
	private bool escapeNext = false;
	private bool setFirst = false;
	public int firstLine = -1;
	public int firstCol = -1;
	
	//the first " gets ignored by the code that creates us. That's not ideal for clarity of ownership, but its fine i guess.
	public override void Consume(char c, int line, int col)
	{
		//mark the string by it's start point for errors, not it's end point.
		if (!setFirst)
		{
			firstLine = line;
			firstCol = col;
			setFirst = true;
		}
		
		//we last was /, then consume the following character no matter what.
		if (escapeNext)
		{
			_builder.Append(c);
			escapeNext = false;
			return;
		}
		
		if (c == '\'')
		{
			escapeNext = true;
			return;
		}

		if (c == '"')
		{
			context.AddToken(new Token(TokenType.String,_builder.ToString(),firstLine,firstCol));
			context.ExitState(this);
			//ignore this closign " in order to consume it.
		}

		_builder.Append(c);
	}
}