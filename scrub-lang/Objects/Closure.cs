namespace scrub_lang.Objects;

public class Closure : Object
{
	public Object[] FreeVariables => _freeVariables;
	private Object[] _freeVariables;
	
	public Function CompiledFunction => _compiledFunction;
	private Function _compiledFunction;

	public Closure(Function function)
	{
		_compiledFunction = function;
		_freeVariables = [];
	}

	public Closure(Function function, Object[] free)
	{
		_compiledFunction = function;
		_freeVariables = free;
	}

	public override ScrubType GetType() => ScrubType.Closure;
	public override string ToString()
	{
		return CompiledFunction.ToString();
	}
}