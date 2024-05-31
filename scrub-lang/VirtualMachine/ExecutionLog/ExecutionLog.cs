namespace scrub_lang.VirtualMachine.ExecutionLog;

public class ExecutionLog 
{
	//todo: This needs to get sent into the VM by reference, so we can keep a non-null version around in the runner. 
	//A linear ... and tree-based ... execution for the VM.
	public List<OpLog> Log => _log;
	private List<OpLog> _log = new List<OpLog>();
	private int logCount;
	public int LogPointer => _logPointer;
	// public ArraySegment<OpLog> ExecutedList = new ArraySegment<OpLog>(_log, 0, 0);
	private int _logPointer;//points to the next free spot.
	public void AddOperation(int depth, string name, string input, string output)
	{
		if (_logPointer >= logCount)
		{
			_log.Add(new OpLog(_log.Count, depth, name, input, output));//hmmm
		}
		else
		{
			_log[_logPointer] = new OpLog(_log.Count, depth, name, input, output);
			_logPointer++;
		}
	}

	public void RemoveOperation()
	{
		if (_logPointer > 0)
		{
			_log.RemoveAt(_log.Count-1);
			_logPointer--;
		}
		else
		{
			//UHOH
			//throw new IndexOutOfRangeException("Cannot remove operation, there are no more operatoins.");
		}
	}
}