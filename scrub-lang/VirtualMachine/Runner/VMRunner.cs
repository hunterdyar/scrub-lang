using System.Text;
using scrub_lang.Compiler;
using scrub_lang.Evaluator;
using scrub_lang.Parser;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.VirtualMachine;
using Environment = scrub_lang.Evaluator.Environment;
//Interface into a machine (or consecutive VM objects)'s with a r-e-p-loop).

public class VMRunner
{
	//Actions
	public Action OnPaused;
	public Action OnComplete;
	public Action OnError;
	public Action OnOutput;
	//core references
	public Status Status => _status;
	private Status _status;
	private VM? _vm;
	private Environment _environment;
	public StringBuilder Output => _output;
	private StringBuilder _output = new StringBuilder();
	private Object[] Globals;
	//convenience
	public VMState? State => _vm?.State;

	//loads but does not start executing. Resets previous state.
	public void CompileProgram(string program, Environment? env = null)
	{
		if (env == null)
		{
			env = new Environment();
		}

		IExpression root;
		try
		{
			root = Scrub.Parse(program);
		}
		catch (ParseException pe)
		{
			_output.AppendLine("Parse Error:" + pe.Message);
			OnOutput?.Invoke();
			return;
		}
		var compiler = new Compiler.Compiler(env);
		try
		{
			var result = compiler.Compile(root);
			if (result != null)
			{
				_output.AppendLine("Compile Error:" + result.Message);
				OnOutput?.Invoke();
				// OnError?.Invoke();
				return;
			}
		}
		catch (CompileException)
		{
			throw new CompileException("i need to change how these errors get returned still");
		}
		//
		//todo: use some kind of streamwriter here. file and log file?
		_vm = new VM(compiler.GetProgram());
		_environment = compiler.Environment();
		_status = new Status(_vm);
	}

	public void RunWithEnvironment(string program)
	{
		CompileProgram(program, _environment);
		if (_vm != null){
			_vm.SetGlobals(Globals);
			RunUntilStop();
		}
	}
	
	/// <summary>
	/// Compiles and then Runs.
	/// </summary>
	public void Run(string program)
	{
		CompileProgram(program);
		if (_vm != null)
		{
			RunUntilStop();
		}
	}

	public void RunUntilStop()
	{
		if (_vm == null)
		{
			throw new VMException("VM in runner is null. Program must be compiled before it can be run.");
		}
		var res = _vm.Run();
		if (res != null)
		{
			//report.
			_output.AppendLine(res.Message);
			OnOutput?.Invoke();
		}
		else
		{
			var lastPopped = _vm.LastPopped();
			if (lastPopped != null)
			{
				_output.AppendLine(lastPopped.ToString());
			}
			else
			{
				_output.AppendLine("Finished.");
			}
			OnOutput?.Invoke();
		}
		Globals = _vm.Globals;//save for REPL oop.s
		
		///
		if (State == VMState.Paused)
		{
			OnPaused?.Invoke();
		}else if (State == VMState.Complete)
		{
			OnComplete?.Invoke();
		}else if (State == VMState.Error)
		{
			OnError?.Invoke();
		}
		else
		{
			throw new VMException("Something has gone wrong");
		}
	}

	public void RunNextExpression()
	{
		//uh... keep running until we hit some special 'expressionbreak' on the same frame we are on.
		//that's the best I got for now lol.
	}
	public void RunNextOperation()
	{
		if (_vm == null)
		{
			throw new NullReferenceException("Cannot advance a program that has not started");
			return;
		}
		
		if (_vm.State == VMState.Paused)
		{
			_vm.RunOne();
			//todo: results.
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