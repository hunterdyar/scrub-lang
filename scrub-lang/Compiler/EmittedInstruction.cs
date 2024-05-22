namespace scrub_lang.Compiler;

public struct EmittedInstruction
{
	public readonly OpCode Op;
	public readonly int Position;

	public EmittedInstruction(OpCode op, int position)
	{
		Op = op;
		Position = position;
	}
}