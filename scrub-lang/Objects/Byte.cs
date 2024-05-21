namespace scrub_lang.Objects;

public class Byte : Objects.Object
{
	public override ScrubType GetType() => ScrubType.Byte;
	public byte GetByte => Bytes[0];

	public Byte(byte[] bytes)
	{
		Bytes = bytes;
	}

	public override string ToString()
	{
		return GetByte.ToString();
	}
}