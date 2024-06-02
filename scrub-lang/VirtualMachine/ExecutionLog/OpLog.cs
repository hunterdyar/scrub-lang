using System.Text;

namespace scrub_lang.VirtualMachine.ExecutionLog;

public struct OpLog
{
	public readonly int OpNumber;
	public readonly int ExecutionDepth;
	public readonly string Input;
	public readonly string OpName;
	public readonly string Output;

	public OpLog(int num, int depth, string name, string input, string output)
	{
		OpNumber = num;
		ExecutionDepth = depth;
		OpName = name;
		Input = input;
		Output = output;
	}
	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < ExecutionDepth; i++)
		{
			sb.Append("  ");
		}
		
		sb.Append(OpName);
		sb.Append(" (");
		sb.Append(Input);
		sb.Append(") -> ");
		sb.Append(Output);
		return sb.ToString();
	}
}