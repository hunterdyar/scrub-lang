using scrub_lang.Compiler;

namespace scrub_lang.Evaluator;

//todo: refactor this. I think we are passing two symbol tables around? 
public class Environment
{
	public SymbolTable? SymbolTable;
	public List<Objects.Object>? Constants;
}