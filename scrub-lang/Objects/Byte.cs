using System.Text;

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
		//assuming that the first 3 bytes are empty is maybe a mistake.
		return Encoding.UTF8.GetString(Bytes);
	}
}