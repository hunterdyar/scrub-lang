using System.Collections;
using scrub_lang.VirtualMachine;

namespace scrub_lang.Objects;

public class Integer : Object
{
	public override ScrubType GetType() => ScrubType.Int;
	public int NativeInt => AsNativeInt();
	public Integer(int nativeInt)
	{
		var bytes = BitConverter.GetBytes(nativeInt);
		if (BitConverter.IsLittleEndian)
		{
			//hmmmm. This right? we should reverse byte-wise before storing the bits.
			bytes = bytes.Reverse().ToArray();
		}
		Bits = new BitArray(bytes);
	}

	public Integer(BitArray bits)
	{
		Bits = bits;
	}

	public Integer(Integer i)
	{
		Bits = i.Bits;
	}
	public int AsNativeInt()
	{
		var bytes = new byte[(Bits.Length - 1) / 8 + 1];
		Bits.CopyTo(bytes, 0);
		return BitConverter.ToInt32([bytes[3],bytes[2],bytes[1],bytes[0]]);//i haven't done performance analyzing but this sure does FEEl faster than a linq query.
	}
	
	public override string ToString()
	{
		return NativeInt.ToString();
	}

	#region Operators

	

	public static Integer operator +(Integer a) => a;

	public static Integer operator +(Integer a, Integer b)
	{
		//fucket, let's just use native C# for the math.
		return new Integer(a.NativeInt + b.NativeInt);
	}

	public static Integer operator -(Integer a) => new Integer(-a.NativeInt);

	public static Integer operator -(Integer a, Integer b)
	{
		return new Integer(a.NativeInt - b.NativeInt);
	}
	
	public static Integer operator *(Integer a, Integer b)
	{
		return new Integer(a.NativeInt * b.NativeInt);
	}

	public static Integer operator /(Integer a, Integer b)
	{
		if (b.NativeInt == 0)
		{
			throw new VMException("Can't Divide By Zero! This is how you break computers. You want to break computer? This is how! Stop!");
		}
		return new Integer(a.NativeInt / b.NativeInt);
	}

	public static Integer operator %(Integer a, Integer b)
	{
		return new Integer(a.NativeInt % b.NativeInt);
	}

	public static Integer operator &(Integer a, Integer b)
	{
		var newInt = new Integer(a.Bits);
		a.Bits.And(b.Bits); 
		return newInt;
	}

	public static Integer operator |(Integer a, Integer b)
	{
		var newInt = new Integer(a.Bits);
		a.Bits.Or(b.Bits);
		return newInt;
	}

	public static Integer operator ^(Integer a, Integer b)
	{
		var newInt = new Integer(a.Bits);
		a.Bits.Xor(b.Bits);
		return newInt;
	}

	public static Integer operator ~(Integer a)
	{
		var newInt = new Integer(a.Bits);
		a.Bits.Not();
		return newInt;
	}
	
	#endregion
}