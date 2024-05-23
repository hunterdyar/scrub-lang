using System.Collections;

namespace scrub_lang.Objects;

public abstract class Object
{
	public BitArray Bits { get; protected set; }
	public byte[] Bytes { get; protected set; }
	public abstract ScrubType GetType();
	
	//todo: Implement bitwise operators here, on bytes. so you can do bitwise on any type!
	public virtual byte[] Concatenate(byte[][] bytesbytes)
	{
		var Bytes = new byte[bytesbytes.Sum(x => x.Length)];
		int offset = 0;
		for (int i = 0; i < bytesbytes.Length; i++)
		{
			bytesbytes[i].CopyTo(Bytes, offset);
			offset += bytesbytes[i].Length;
		}

		return Bytes;
	}


	public Object(byte[] data)
	{
		Bytes = data;
	}

	public Object()
	{
	}

	public virtual String ToScrubString()
	{
		return new String(this.ToString());
	}

	#region BitwiseOps

	// public static Object operator &(Object a, Object b)
	// {
	// 	return new Object(a.Bytes & b.Bytes);
	// }

	#endregion
}