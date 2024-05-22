using scrub_lang.Objects;

namespace scrub_lang.VirtualMachine;

public class Frame
{
	/// <summary>
	/// The compiled Instructions
	/// </summary>
	public Function fn;
	//compiled instruction pointer.s
	public int ip;

	public Frame(Function fn)
	{
		this.fn = fn;
		ip = -1;
	}

	public byte[] Instructions()
	{
		return fn.Bytes;
	}
}