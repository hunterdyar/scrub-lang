namespace scrub_lang.Objects;

public abstract class Object
{
	public byte[] Bytes { get; protected set; }
	public abstract ScrubType GetType();
	
	//todo: Implement bitwise operators here, on bytes. so you can do bitwise on any type!
}