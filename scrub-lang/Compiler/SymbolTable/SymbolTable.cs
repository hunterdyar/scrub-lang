using scrub_lang.VirtualMachine;

namespace scrub_lang.Compiler;

public class SymbolTable
{
	public SymbolTable? Outer;
	public const string GlobalScope = "GLOBAL";
	public const string LocalScope = "LOCAL";
	public const string BuiltInScope = "BUILTIN";
	
	public Dictionary<string, Symbol> Table = new Dictionary<string, Symbol>();//todo: enums for scope?
	public int NumDefinitions => _numDefinitions;
	private int _numDefinitions = 0;
	
	public Symbol Define(string name)
	{
		Symbol s = new Symbol(name, _numDefinitions, Outer == null ? GlobalScope : LocalScope);
		Table.Add(name,s);
		_numDefinitions++;
		return s;
	}

	public Symbol DefineBuiltin(int index, string name)
	{
		var s = new Symbol(name, index, BuiltInScope);
		this.Table[name] = s;
		return s;
	}
	
	public bool TryResolve(string name, out Symbol s, bool recurse = true)
	{
		//error handling
		if (Table.TryGetValue(name, out s))
		{
			return true;
		}

		//recursively check outer symbols.
		if (recurse && Outer != null)
		{
			return Outer.TryResolve(name, out s);
		}
		//we are global and there is no symbol. give up.
		return false;
	}

	//this is a pretty direct traslation of hte go monkey code, but we can handle it more elequently. todo: refactor.
	public SymbolTable NewEnclosedSymbolTable()
	{
		var s = new SymbolTable();
		s.Outer = this;
		return s;
	}
}