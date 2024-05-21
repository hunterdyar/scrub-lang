using scrub_lang.Memory;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.Compiler;
using Instructions = byte[];
public class ByteCode
{
	public Instructions Instructions;
	public Object[] Constants;

	public ByteCode(Instructions instructions, Object[] constants)
	{
		Instructions = instructions;
		Constants = constants;
	}
}
