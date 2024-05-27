using scrub_lang.Objects;

namespace scrub_lang.Evaluator;

public class ScrubMemoryError : ScrubError
{
	public ScrubMemoryError(string message) : base(message)
	{
	}

	public override ScrubType GetType()
	{
		return ScrubType.Error;
	}
}