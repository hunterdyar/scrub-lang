﻿using scrub_lang.Objects;

namespace scrub_lang.VirtualMachine;

public class Frame
{
	/// <summary>
	/// The compiled Instructions
	/// </summary>
	public Closure closure;
	//compiled instruction pointer.
	public int ip;
	public int basePointer;

	public Frame(Closure closure, int basePointer, int startIp = 0)
	{
		this.closure = closure;
		this.ip = startIp;//would be -1 but we put a return at the beginning of the closure.
		this.basePointer = basePointer;
	}

	public byte[] Instructions()
	{
		return closure.CompiledFunction.CompiledFunction;
	}
}