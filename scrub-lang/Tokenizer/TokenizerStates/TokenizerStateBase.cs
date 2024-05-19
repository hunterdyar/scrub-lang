namespace scrub_lang.Tokenizer;

public abstract class TokenizerStateBase(Tokenizer context)
{
	protected Tokenizer context = context;

	public abstract void Consume( char c, int line, int col);
}