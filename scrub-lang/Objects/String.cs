using System.Text;

namespace scrub_lang.Objects;

public class String : Objects.Object
{
	public override ScrubType GetType() => ScrubType.String;

	public String(string input)
	{
		Bytes = Encoding.UTF8.GetBytes(input);
	}

	public string ToNativeString()
	{
		return Encoding.UTF8.GetString(Bytes);
	}
	//THis is a nifty hack of the C# params feature that takes any number of individual or array inputs and gives you a single array of them. 
	//we are using it to create a string that can be assembled from characters.s
	public String(params byte[] bytes)
	{
		Bytes = bytes;
	}

	//a string that can be assembled from other strings (concatenation)
	public String(params byte[][] bytesbytes)
	{
		//hmmmm
		Bytes = Concatenate(bytesbytes);
	}

	//Makes annoying compiler error go away. yes, yes, FINE children of string could bork everything. i don't have any, it's fine.
	public sealed override byte[] Concatenate(byte[][] bytesbytes)
	{
		return base.Concatenate(bytesbytes);
	}

	public override string ToString()
	{
		return ToNativeString();
	}

	public override String ToScrubString()
	{
		return this;
	}
}