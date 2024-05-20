namespace scrub_lang.Tokenizer;

public class StreamProvider : ISourceProvider
{
	private StreamReader _streamReader;

	public StreamProvider(StreamReader streamReader)
	{
		_streamReader = streamReader;
	}

	public bool TryGetNextCharacter(out char c, ref int line, ref int col)
	{
		int next = _streamReader.Read();
		if(next == -1)
		{
			c = Char.MinValue;
			return false;
		}

		c = (char)next;
		col = col + 1;
		if (c == '\n')
		{
			line++;
			col = 0;
		}
		return true;
	}
}