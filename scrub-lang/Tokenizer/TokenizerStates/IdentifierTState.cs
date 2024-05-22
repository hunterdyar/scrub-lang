using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

public class IdentifierTState(Tokenizer context) : TokenizerStateBase(context)
{
	private StringBuilder _builder = new StringBuilder();
	private int firstLine = -1;
	private int firstCol = -1;

	private readonly Dictionary<string, TokenType> Literals = new Dictionary<string, TokenType>()
	{
		{"if",TokenType.IfKeyword},
		{"while",TokenType.WhileKeyword},
		{"func",TokenType.FunctionKeyword},
		{"true",TokenType.TrueKeyword},
		{"false",TokenType.FalseKeyword},
		{"return", TokenType.ReturnKeyword},
		{"else",TokenType.ElseKeyword},
	};
	public override void Consume(char c, int line, int col)
	{
		if (firstLine < 0)
		{
			firstLine = line;
			firstCol = col;
		}
		
		if (Char.IsWhiteSpace(c) || (!char.IsLetter(c) && !char.IsDigit(c)))
		{
			var literal = _builder.ToString();
			CreateAndAddToken(literal);
			context.ExitState(this);
			//someone still needs to consume this whatever-it-is.
			context.ConsumeNext(c,line,col);
			return;
		}
		//else... 
		_builder.Append(c);
	}

	private void CreateAndAddToken(string literal)
	{
		if(Literals.TryGetValue(literal, out var tt))
		{
			context.AddToken(new Token(tt,literal,firstLine,firstCol));
		}
		else
		{
			context.AddToken(new Token(TokenType.Identifier, literal, firstLine, firstCol));
		}
	}
}