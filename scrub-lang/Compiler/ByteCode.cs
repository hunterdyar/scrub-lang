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
	public ByteCode(int[] instructions, Object[] constants, OpLocationLookup locationLookup, int numSymbols)
	{
		Lookup = locationLookup;
		Instructions = instructions;
		Constants = constants;
		NumSymbols = numSymbols;
	}
}
