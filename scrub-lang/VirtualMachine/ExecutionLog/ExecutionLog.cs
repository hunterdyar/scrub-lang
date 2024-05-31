namespace scrub_lang.VirtualMachine.ExecutionLog;

public class ExecutionLog 
{
	//todo: This needs to get sent into the VM by reference, so we can keep a non-null version around in the runner. 
	//A linear ... and tree-based ... execution for the VM.
	public List<OpLog> Log => _log;
	private List<OpLog> _log = new List<OpLog>();
	private int logCount;
	private int logPointer;//points to the next free spot.
	public void AddOperation(int depth, string name, string input, string output)
	{
		if (logPointer >= logCount)
		{
			_log.Add(new OpLog(_log.Count, depth, name, input, output));
		}
		else
		{
			_log[logPointer] = new OpLog(_log.Count, depth, name, input, output);
			logPointer++;
		}
	}

	public void RemoveOperation()
	{
		if (logPointer > 0)
		{
			logPointer--;
		}
		else
		{
			throw new IndexOutOfRangeException("Cannot remove operation, there are no more operatoins.");
		}
	}
}