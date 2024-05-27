namespace scrub_lang.Compiler;

public class CompilationScope
{
	public List<int> Instructions = new List<int>();
	public OpLocationLookup OpLocationLookup = new OpLocationLookup();
	public EmittedInstruction LastInstruction;//last instruction
	public EmittedInstruction PreviousInstruction;//the one before lastInstructin.
	//public Dictionary<int, Location> LocationTable = new Dictionary<int, Location>(); //connects instucton
	public int AddInstruction(int instruction, Location loc)
	{
		int pos = Instructions.Count;
		Instructions.Add(instruction);
		OpLocationLookup.Add(pos,loc);
		return pos;
	}
}