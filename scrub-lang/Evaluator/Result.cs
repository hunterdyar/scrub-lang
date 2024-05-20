using scrub_lang.Memory;

namespace scrub_lang.Evaluator;

public class Result
{
	public ScrubObject? ScrubObject;
	public ScrubRuntimeError? Error;
	public bool HasError => Error != null;
	public bool HasObject => ScrubObject != null;

	public Result(ScrubObject? scrubObject, ScrubRuntimeError? error)
	{
		ScrubObject = scrubObject;
		Error = error;
	}

	public Result(ScrubObject scrubObject)
	{
		ScrubObject = scrubObject;
		Error = null;
	}

	public Result(ScrubRuntimeError error)
	{
		ScrubObject = null;
		Error = error;
	}
	public Result()
	{
		ScrubObject = null;
		Error = null;
	}

	

	public override string ToString()
	{
		if (!HasError)
		{
			return ScrubObject.ToString();
		}
		else
		{
			return Error.ToString();
		}
	}
}