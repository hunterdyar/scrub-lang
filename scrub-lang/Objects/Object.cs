using System.Collections;

namespace scrub_lang.Objects;

public abstract class Object
{
	public BitArray Bits { get; protected set; }
	public abstract ScrubType GetType();
	
	//todo: Implement bitwise operators here, on bytes. so you can do bitwise on any type!
	public virtual BitArray Concatenate(BitArray[] bitsArrays)
	{
		var concatBits = new BitArray[bitsArrays.Sum(x => x.Length)];
		int offset = 0;
		
		//todo: there has to be some horribly clever way to append a series of bitarrays together.
		for (int i = 0; i < bitsArrays.Length; i++)
		{
			for (int b = 0; b < bitsArrays[i].Length; b++)
			{
				concatBits.SetValue(bitsArrays[i][b],offset+i);
			}
		}

		return Bits;
	}


	public Object(BitArray data)
	{
		Bits = data;
	}

	public Object()
	{
		Bits = new BitArray(8);
	}

	public virtual String ToScrubString()
	{
		return new String(this.ToString());
	}

	public byte[] AsByteArray()
	{
		var bytes = new byte[(Bits.Length - 1) / 8 + 1];
		Bits.CopyTo(bytes,0);
		return bytes;
	}
	
	#region BitwiseOps

	// public static Object operator &(Object a, Object b)
	// {
	// 	return new Object(a.Bytes & b.Bytes);
	// }

	#endregion
}