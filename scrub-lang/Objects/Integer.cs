using System.Collections;
using scrub_lang.VirtualMachine;

namespace scrub_lang.Objects;

public class Integer : Object
{
	public override ScrubType GetType() => ScrubType.Int;
	public int NativeInt { get; protected set; }
	public Integer(int nativeInt)
	{
		NativeInt = nativeInt;//that's right for every integer we store two integers. Don't think about it too much the memory overhead is worth saving casting, but I insist on having accessible and properly formatted byte[] data. 
		var bytes = BitConverter.GetBytes(nativeInt);
		if (BitConverter.IsLittleEndian)
		{
			//hmmmm. This right?
			bytes = bytes.Reverse().ToArray();
		}
		Bits = new BitArray(bytes); //todo: this will make integers always 32 bits, which i think is desired.

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
	
	#endregion
}