namespace scrub_lang.Evaluator;

public abstract class ScrubError
{
	public readonly string Message;
	
	public ScrubError(string message)
	{
		Message = message;
	}

	public override string ToString()
	{
		return Message;
	}
}