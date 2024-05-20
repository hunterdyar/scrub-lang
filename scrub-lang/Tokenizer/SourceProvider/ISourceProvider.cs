namespace scrub_lang.Tokenizer;

public interface ISourceProvider
{
		public bool TryGetNextCharacter(out char c, ref int line, ref int col);
}