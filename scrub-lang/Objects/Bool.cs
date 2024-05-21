namespace scrub_lang.Objects;

public class Bool : Objects.Object
{
	public override ScrubType GetType() => ScrubType.Bool;
	public bool NativeBool { get; protected set; } 
	public Bool(bool b)
	{
		NativeBool = b;
		Bytes = BitConverter.GetBytes(b);
	}
}