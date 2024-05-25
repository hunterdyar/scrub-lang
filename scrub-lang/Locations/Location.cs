namespace scrub_lang;

public struct Location
{
	public int Line;
	public int Column;

	//todo: need to be able to represent an invalid location
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