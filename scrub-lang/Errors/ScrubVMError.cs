using scrub_lang.Objects;

namespace scrub_lang.Evaluator;

public class ScrubVMError : ScrubError
{
	public ScrubVMError(string message) : base(message)
	{
	}

	public override ScrubType GetType()
	{
		return ScrubType.Error;
	}
}