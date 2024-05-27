namespace scrub_lang.Compiler;

public struct Symbol
{
	public string Name;
	public ScopeDef Scope;
	public int Index;

	public Symbol(string name, int index, ScopeDef scope)
	{
		Name = name;
		Index = index;
		Scope = scope;
	}
}