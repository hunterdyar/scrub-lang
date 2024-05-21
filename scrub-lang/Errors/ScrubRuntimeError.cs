using scrub_lang.Parser;

namespace scrub_lang.Evaluator;

public class ScrubRuntimeError : ScrubError
{
	public string Message;
	public IExpression Expression;

	public ScrubRuntimeError(string message) : base(message)
	{
	}

	public ScrubRuntimeError(string message, IExpression expression) : base(message)
	{
		Expression = expression;
	}
	
}