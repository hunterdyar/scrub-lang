namespace scrub_lang.Objects;

public class Pointer : Object
{
	private Object o;

	public Pointer(Object obj)
	{
		o = obj;
	}
	public override ScrubType GetType()
	{
		return ScrubType.Pointer;
	}

	public Object GetObject()
	{
		return o;
	}

	public static Object Resolve(Object o)
	{
		if (o is Pointer p)
		{
			return p.GetObject();
		}

		return o;
	}

	public override string ToString()
	{
		return o.ToString();
	}

	public override String ToScrubString()
	{
		return o.ToScrubString();
	}
}