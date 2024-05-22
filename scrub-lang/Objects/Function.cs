namespace scrub_lang.Objects;

//A function Object is a -compiled- function.
public class Function : Object
{
	public Function (byte[] instructions)
	{
		Bytes = instructions;
	}

	public override ScrubType GetType() => ScrubType.Function;

	public override string ToString()
	{
		return $"Compiled Function: [{Bytes.ToDelimitedString(",")}]";
	}
}