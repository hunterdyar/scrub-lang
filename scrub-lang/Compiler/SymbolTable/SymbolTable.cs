using System.Diagnostics;
using scrub_lang.VirtualMachine;

namespace scrub_lang.Compiler;

public class SymbolTable
{
	public SymbolTable? Outer;
	public const string GlobalScope = "GLOBAL";
	public const string LocalScope = "LOCAL";
	public const string BuiltInScope = "BUILTIN";
	public const string FunctionScope = "FUNCTION";
	public const string FreeScope = "FREE";//every non-global non-local non-built-in is free. e.g. local variables from enclosing scopes. ('free' is a relative term. free in one scope could be local in an enclosing).
	public Dictionary<string, Symbol> Table = new Dictionary<string, Symbol>();//todo: enums for scope?
	public List<Symbol> FreeTable = new List<Symbol>();
	
	public int NumDefinitions => _numDefinitions;
	private int _numDefinitions = 0;
	public int NumFree => _numFree;
	private int _numFree = 0;

	
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

	public Symbol DefineFunctionName(string name)
	{
		var s = new Symbol(name, 0, FunctionScope);//it's always 0... there will only ever be one functon scope. we could choose any number, but that would be dumb.
		this.Table[name] = s;
		return s;
	}

	//Adds  symbol to freeSymbols, and returns a freescope version of it.
	public Symbol DefineFree(Symbol original)
	{
		FreeTable.Add(original);
		var newFree = new Symbol(original.Name, _numFree , FreeScope);
		_numFree++;

		Table[original.Name] = newFree;
		//numDefinitions doesn't increment because, uh,
		return newFree;
	}
	
	public bool TryResolve(string name, out Symbol s, bool recurse = true)
	{
		//error handling
		if (Table.TryGetValue(name, out s))
		{
			//it's local! easy. got it.
			return true;
		}

		//recursively check outer symbols.
		if (recurse && Outer != null)
		{
			//is defined in outer
			if (!Outer.TryResolve(name, out s))
			{
				return false;
			}
			//is it a global? or a built-in? then we got the right one.
			if (s.Scope == GlobalScope || s.Scope == BuiltInScope)
			{
				return true;
			}
			
			//if not our local, global, or built-in... it must be a free variable. e.g. another scopes local.
			//have a version of that symbol, copied and in the free scope.
			s = DefineFree(s);
			return true;
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