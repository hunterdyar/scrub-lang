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
	public int basePointer;

	public Frame(Function fn, int basePointer)
	{
		this.fn = fn;
		ip = -1;
		this.basePointer = basePointer;
	}

	public byte[] Instructions()
	{
		return fn.Bytes;
	}
}