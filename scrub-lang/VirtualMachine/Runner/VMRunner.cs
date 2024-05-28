namespace scrub_lang.VirtualMachine;

//Interface into a machine (or consecutive VM objects)'s with a r-e-p-loop).

public class VMRunner
{
	//core references
	public Status Status => _status;
	private Status _status;
	private VM? _vm;
	
	//convenience
	public VMState State => _vm.State;

	//loads but does not start executing. Resets previous state.
	public void CompileProgram(string program)
	{
		var root = Scrub.Parse(program);
		var prog = Scrub.Compile(root);
		_vm = new VM(prog);
	}
	
	/// <summary>
	/// Compiles and then Runs.
	/// </summary>
	public void Run(string program)
	{
		CompileProgram(program);
		RunUntilStop();
	}

	public void RunUntilStop()
	{
		_vm.Run();
	}

	public void NextExpression()
	{
		//uh... keep running until we hit some special 'expressionbreak' on the same frame we are on.
		//that's the best I got for now lol.
	}
	public void NextOp()
	{
		if (_vm == null)
		{
			throw new NullReferenceException("Cannot advance a program that has not started");
			return;
		}
		
		if (_vm.State == VMState.Paused)
		{
			_vm.RunOne();
		}
	}
	
	public void UndoOneOp()
	{
		if (_vm == null)
		{
			throw new NullReferenceException("Cannot revert a program that has not started");
			return;
		}

		if (_vm.State == VMState.Paused)
		{
			_vm.PreviousOne();
		}
	}
}