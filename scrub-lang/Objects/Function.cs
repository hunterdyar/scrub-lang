namespace scrub_lang.Objects;

//A function Object is a -compiled- function.
public class Function : Object
{
	public int NumLocals;//
	public Function(byte[] instructions, int numLocals)
	{
		Bytes = instructions;
		NumLocals = numLocals;
	}

	public override ScrubType GetType() => ScrubType.Function;

	public override string ToString()
	{
		return $"Compiled Function: [{Bytes.ToDelimitedString(",")}]";
	}
}