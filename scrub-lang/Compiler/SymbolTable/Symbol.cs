namespace scrub_lang.Compiler;

public struct Symbol
{
	public string Name;
	public string Scope;
	public int Index;

	public Symbol(string name, int index, string scope)
	{
		Name = name;
		Index = index;
		Scope = scope;
	}
}