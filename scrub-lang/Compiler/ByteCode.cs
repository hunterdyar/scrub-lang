using scrub_lang.Memory;

namespace scrub_lang.Compiler;
using Instructions = byte[];
public class ByteCode
{
	public Instructions Instructions;
	public object[] Constants;

	public ByteCode(Instructions instructions, object[] constants)
	{
		Instructions = instructions;
		Constants = constants;
	}
}
