using System.Collections;
using System.Text;

namespace scrub_lang.Objects;

public class String : Objects.Object
{
	public override ScrubType GetType() => ScrubType.String;

	public String(string input)
	{
		var utf = Encoding.UTF8.GetBytes(input);
		Bits = new BitArray(utf);
	}

	public string ToNativeString()
	{
		return Encoding.UTF8.GetString(AsByteArray());
	}
	//THis is a nifty hack of the C# params feature that takes any number of individual or array inputs and gives you a single array of them. 
	//we are using it to create a string that can be assembled from characters.s
	public String(BitArray bits)
	{
		Bits = bits;
	}

	//a string that can be assembled from other strings (concatenation)
	public String(params BitArray[] bitsybitss)
	{
		//hmmmm
		Bits = Concatenate(bitsybitss);
	}

	public String(params String[] stringsys)
	{
		//hmmmm
		if (stringsys.Length == 1)
		{
			Bits = stringsys[0].Bits;
		}
		else
		{
			Bits = Concatenate(stringsys.Select(x => x.Bits).ToArray());
		}
	}

	//Makes annoying compiler error go away. yes, yes, FINE children of string could bork everything. i don't have any, it's fine.
	public sealed override BitArray Concatenate(BitArray[] bitArrays)
	{
		return base.Concatenate(bitArrays);
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