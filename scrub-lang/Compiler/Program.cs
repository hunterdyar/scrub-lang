using Object = scrub_lang.Objects.Object;

namespace scrub_lang.Compiler;
//wrapper for grabbing the things the compiler generates. Except not all of it because I need to refactor.
public class Program
{
	public int[] Instructions;
	public Object[] Constants;
	public OpLocationLookup Lookup;
	public SymbolTable Symbols;
	//and we have to pass the opLocationLookup to the VM for errors, and the op and symbol table to the Report for... well for knowing what the names of the variables are.
	//we also need to figure out how to go from 'this frame' to 'this local stack pointer' to the correct variable.
	public Program(int[] instructions, Object[] constants, OpLocationLookup locationLookup, SymbolTable symbols)
	{
		Lookup = locationLookup;
		Instructions = instructions;
		Constants = constants;
		Symbols = symbols;
	}
}
