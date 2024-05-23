using scrub_lang.Objects;

namespace scrub_lang.VirtualMachine;

public class Frame
{
	/// <summary>
	/// The compiled Instructions
	/// </summary>
	public Closure closure;
	//compiled instruction pointer.s
	public int ip;
	public int basePointer;

	public Frame(Closure closure, int basePointer)
	{
		this.closure = closure;
		ip = -1;
		this.basePointer = basePointer;
	}

	public byte[] Instructions()
	{
		return closure.CompiledFunction.Bytes;
	}
}