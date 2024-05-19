using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

public class CommentTState(Tokenizer context) : TokenizerStateBase(context)
{
	private int firstLine = -1;
	private int firstCol = -1;
	private bool firstSlash = false;
	private bool isSingleComment = false;
	private bool isBlockComment = false;
	private bool escapeNext = false;
	private char prevChar;

	public override void Consume(char c, int line, int col)
	{
		if (escapeNext)
		{
			escapeNext = false;
			prevChar = c;//skip all the checks and just go next no matter what when there is a \
			return;
		}

		if (firstLine < 0)
		{
			firstLine = line;
			firstCol = col;
		}

		if (!firstSlash)
		{
			if (c == '/')
			{
				firstSlash = true;
				prevChar = c;
				return; //the next character tells all!
			}

			//else: error! How did we get into this state
			Console.WriteLine("tokenization error. How did we get into comment/slash state without starting with a slash?");
			return;
		}

		if (isSingleComment)
		{
			if (c == '\n')
			{
				//we don't bother saving the comments. 
				context.ExitState(this);
				return;
				// context.ConsumeNext(c,line,col);//i dont thing we need to do this...
			}
		}

		if (isBlockComment)
		{
			if (c == '\'')
			{
				escapeNext = true;
				prevChar = c;
				return;
			}

			//the block comment has ended */ (with neither escaped by a \
			if (prevChar == '*' && c == '/')
			{
				context.ExitState(this);
				return;
			}
		}

		if (prevChar == '/' && c == '/')
		{
			isSingleComment = true;
		}
		else if (prevChar == '/' && c == '*')
		{
			isBlockComment = true;
		}
		
		//if it's not a block comment or a single line comment (yet)
		if (!isSingleComment && !isBlockComment && prevChar == '/' && (c != '/' || c != '*')) //&prevChar == '/'
		{
			context.AddToken(new Token(TokenType.Division, prevChar, firstLine, firstCol));
			context.ExitState(this);
			//someone still needs to consume this whatever-it-is..
			context.ConsumeNext(c, line, col);
			return;
		}

		

		prevChar = c;
	}
	
}