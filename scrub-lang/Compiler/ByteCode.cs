using scrub_lang.Memory;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.Compiler;
//wrapper for grabbing the things the compiler generates. Except not all of it because I need to refactor.
public class ByteCode
{
	public int[] Instructions;
	public Object[] Constants;
	public OpLocationLookup Lookup;
	public int NumSymbols;
	//todo: This is going to need to change to 'program' or something like that, becuase it's way more than just the bytecode.
	//and we have to pass the opLocationLookup to the VM for errors, and the op and symbol table to the Report for... well for knowing what the names of the variables are.
	//we also need to figure out how to go from 'this frame' to 'this local stack pointer' to the correct variable.
	public ByteCode(int[] instructions, Object[] constants, OpLocationLookup locationLookup, int numSymbols)
	{
		Lookup = locationLookup;
		Instructions = instructions;
		Constants = constants;
		NumSymbols = numSymbols;
	}
}
