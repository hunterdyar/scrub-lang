namespace scrub_lang.Objects;

//A function Object is a -compiled- function.
public class Function : Object
{
	public string Name = "";
	public byte[] CompiledFunction;//too slow to use our bitarray for functions, which are internal only. should we have internal objects not be Scrubobjects?
	public int NumLocals;//
	public Function(byte[] instructions, int numLocals)
	{
		CompiledFunction = instructions;
		NumLocals = numLocals;
	}

	public override ScrubType GetType() => ScrubType.Function;

	public override string ToString()
	{
		return $"Compiled Function: [{CompiledFunction.ToDelimitedString(",")}]" + (string.IsNullOrEmpty(Name) ? "" : $"({Name})"); 
	}
}