namespace scrub_lang.Tokenizer.Tokens;

public class IntegerLiteral(string literal, int line, int column) : TokenBase(literal, line, column)
{
	public int value;
}