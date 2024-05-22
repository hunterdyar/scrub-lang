using scrub_lang.VirtualMachine;

namespace scrub_lang.Compiler;

public class SymbolTable
{
	public const string GlobalScope = "GLOBAL";
	public Dictionary<string, Symbol> Table = new Dictionary<string, Symbol>();
	public int numDefinitions = 0;
	
	public Symbol Define(string name)
	{
		Symbol s = new Symbol(name, numDefinitions, GlobalScope);
		Table.Add(name,s);
		numDefinitions++;
		return s;
	}

	public bool TryResolve(string name, out Symbol s)
	{
		//error handling
		return Table.TryGetValue(name, out s);
	}
}