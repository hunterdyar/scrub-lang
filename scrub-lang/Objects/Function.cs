using scrub_lang.Compiler;

namespace scrub_lang.Objects;

//A function Object is a -compiled- function.
public class Function : Object
{
	public string Name = "";
	public int[] CompiledFunction;//too slow to use our bitarray for functions, which are internal only. should we have internal objects not be Scrubobjects?
	public int NumLocals;//
	public int NumArgs;
	public Function(int[] instructions, int numArgs, int numLocals, bool prependReturn = true)
	{
		if (prependReturn)
		{
			//CompiledFunction = new int[instructions.Length + 1];
			//CompiledFunction[0] = Op.Make(OpCode.OpReturnValue,0,0);//hmmmmmmmmmmmmmmmm
			//instructions.CopyTo(CompiledFunction, 1);
		}
		else
		{
			CompiledFunction =  instructions;
		}

		NumArgs = numArgs;
		NumLocals = numLocals;
	}

	public override ScrubType GetType() => ScrubType.Function;

	public override string ToString()
	{
		if (CompiledFunction == null)
		{
			return "NUll Function";
		}
		return $"Compiled Function: [{CompiledFunction.ToDelimitedString(",")}]" + (string.IsNullOrEmpty(Name) ? "" : $"({Name})"); 
	}
}