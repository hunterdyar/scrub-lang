namespace scrub_lang.Tokenizer.Tokens;

public class Token(TokenType tokenType, string literal, int line, int column)
{
	public readonly TokenType TokenType = tokenType;
	public readonly string Literal = literal;
	public readonly int Line = line;
	public readonly int Column = column;
}