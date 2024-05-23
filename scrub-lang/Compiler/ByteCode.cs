using scrub_lang.Memory;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.Compiler;
using Instructions = byte[];
//wrapper for grabbing the things the compiler generates. Except not all of it because I need to refactor.
public class ByteCode
{
	public Instructions Instructions;
	public Object[] Constants;
	public int NumSymbols;
	public ByteCode(Instructions instructions, Object[] constants, int numSymbols)
	{
		Instructions = instructions;
		Constants = constants;
		NumSymbols = numSymbols;
	}
}
