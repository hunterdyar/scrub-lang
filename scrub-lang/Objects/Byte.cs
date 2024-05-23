using System.Collections;
using System.Text;

namespace scrub_lang.Objects;

public class Byte : Objects.Object
{
	public override ScrubType GetType() => ScrubType.Byte;
	public byte Value; //lol cop-out naming on this one.

	public Byte(byte b)
	{
		Bits = new BitArray(b);
		Value = AsByteArray()[0];
	}

	public override string ToString()
	{
		//assuming that the first 3 bytes are empty is maybe a mistake.
		return Encoding.UTF8.GetString([Value]);
	}
}