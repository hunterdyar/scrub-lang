namespace scrub_lang.Objects;

public class Null : Object
{
	public override ScrubType GetType() => ScrubType.Null;
	public override string ToString()
	{
		return "Nully";//spelld wrong so i can tell when it's this and when it's c#. hah.
	}
}