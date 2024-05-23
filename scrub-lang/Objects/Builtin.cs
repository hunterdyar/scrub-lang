using scrub_lang.VirtualMachine;

namespace scrub_lang.Objects;

public class Builtin : Object
{
	public delegate Object? BuiltInFunction(VM context, params Object[] args);

	public BuiltInFunction Function;

	public Builtin(BuiltInFunction function)
	{
		Function = function;
	}

	public override ScrubType GetType()
	{
		return ScrubType.Builtin;
	}
}