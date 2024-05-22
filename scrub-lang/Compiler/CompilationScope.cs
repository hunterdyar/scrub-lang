namespace scrub_lang.Compiler;

public class CompilationScope
{
	public List<byte> Instuctions = new List<byte>();
	public EmittedInstruction LastInstruction;//last instruction
	public EmittedInstruction PreviousInstruction;//the one before lastInstructin.
}