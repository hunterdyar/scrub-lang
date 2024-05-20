namespace scrub_lang.Tokenizer;

public class StringProvider : ISourceProvider
{
	private int _loc = 0;
	private int _length;
	private string _source;

	public StringProvider(string source)
	{
		_source = source;
		_length = _source.Length;
	}

	public bool TryGetNextCharacter(out char c, ref int line, ref int col)
	{
		_loc++;
		if (_loc >= _length)
		{
			c = Char.MinValue;
			return false;
		}

		c = _source[_loc];
		col++;
		if (c == '\n')
		{
			line++;
			col = 0;
		}
		return true;
	}
}