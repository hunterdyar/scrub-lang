using scrub_lang.Parser;

namespace scrub_lang.Evaluator;

public class ScrubRuntimeError
{
	public string Message;
	public IExpression Expression;

	public ScrubRuntimeError(string message)
	{
		Message = message;
	}

	public ScrubRuntimeError(string message, IExpression expression)
	{
		Message = message;
		Expression = expression;
	}
	
	public override string ToString()
	{
		return Message;
	}
}