namespace scrub_lang.Tokenizer;

public abstract class TokenizerState
{
	public abstract void Consume(ref Tokenizer context, char c);
}