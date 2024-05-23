using System.Collections;

namespace scrub_lang.Objects;

public class Bool : Objects.Object
{
	public override ScrubType GetType() => ScrubType.Bool;
	public bool NativeBool { get; protected set; } 
	public Bool(bool b)
	{
		NativeBool = b;
		Bits = new BitArray(1);
		Bits.Set(0,b);
	}

	public override string ToString()
	{
		return NativeBool ? "true" : "false";
	}
}