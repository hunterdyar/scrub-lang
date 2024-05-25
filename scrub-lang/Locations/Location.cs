namespace scrub_lang;

public struct Location
{
	public int Line;
	public int Column;

	public Location(int line, int column)
	{
		Line = line;
		Column = column;
	}

	public override string ToString()
	{
		return $"{Line}:{Column}";
	}
}