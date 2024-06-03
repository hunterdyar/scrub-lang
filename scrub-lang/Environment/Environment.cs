using scrub_lang.Compiler;

namespace scrub_lang.Evaluator;

//todo: refactor this. We aren't using this (see Program class). Either use it or ... dont use it...
public class Environment
{
	public SymbolTable? SymbolTable;
	public List<Objects.Object>? Constants;
}