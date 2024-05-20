using System.Text;

namespace scrub_lang.Memory;

public class ScrubObject
{
	public byte[] Data;
	public ScrubType ScrubType;

	#region Constructors

	public ScrubObject(bool value)
	{
		ScrubType = ScrubType.sBool;
		Data = BitConverter.GetBytes(value);
	}

	public ScrubObject(int value)
	{
		ScrubType = ScrubType.sInt;
		Data = BitConverter.GetBytes(value);
	}

	public ScrubObject(uint value)
	{
		ScrubType = ScrubType.sUint;
		Data = BitConverter.GetBytes(value);
	}

	public ScrubObject(string value)
	{
		ScrubType = ScrubType.sInt;
		Data = Encoding.UTF8.GetBytes(value);
	}

	public ScrubObject(double value)
	{
		ScrubType = ScrubType.sDouble;
		Data = BitConverter.GetBytes(value);
	}

	#endregion
	
	#region NativeConverters
	public bool ToNativeBool()
	{
		return BitConverter.ToBoolean(Data);
	}

	public int ToNativeInt()
	{
		return BitConverter.ToInt32(Data);
	}

	public uint ToNativeUInt()
	{
		return BitConverter.ToUInt32(Data);
	}

	public double ToNativeDouble()
	{
		return BitConverter.ToDouble(Data);
	}

	#endregion
	
	public override string ToString()
	{
		switch (ScrubType)
		{
			case ScrubType.sBool:
				return  ToNativeBool() ? "true" : "false";
			case ScrubType.sInt:
				return ToNativeInt().ToString();
			case ScrubType.sUint:
				return ToNativeUInt().ToString();
			case ScrubType.sString:
				return Encoding.ASCII.GetString(Data);
			case ScrubType.sDouble:
				return ToNativeDouble().ToString();
		}

		return Data.ToString();
	}
}