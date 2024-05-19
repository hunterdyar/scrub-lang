﻿using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

public class LiteralTState(Tokenizer context) : TokenizerStateBase(context)
{
	private StringBuilder _builder = new StringBuilder();
	private int firstLine;
	private int firstCol;

	private readonly Dictionary<string, TokenType> Literals = new Dictionary<string, TokenType>()
	{
		{"if",TokenType.IfKeyword},
		{"while",TokenType.WhileKeyword},
	};
	public override void Consume(char c, int line, int col)
	{
		
		if (Char.IsWhiteSpace(c) || !char.IsLetter(c) || !char.IsDigit(c))
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