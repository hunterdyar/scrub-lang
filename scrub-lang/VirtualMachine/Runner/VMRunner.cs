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
	public Action<string> OnNewResult;
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
	public Object? LastResult;

	//loads but does not start executing. Resets previous state.
	public void CompileProgram(string program, Environment? env = null)
	{
		if (String.IsNullOrEmpty(program))
		{
			//not an error, but nothing to do.
			return;
		}
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
			_output.AppendLine("Parse Error: " + pe.Message);
			OnOutput?.Invoke();
			return;
		}
		var compiler = new Compiler.Compiler(env);
		if (!compiler.TryCompile(root, out var error, out var prog))
		{
			_output.AppendLine("Compiler Error: " + error.Message);
			OnOutput?.Invoke();
			return;
		}
		//todo: use some kind of streamwriter here. file and log file?
		_vm = new VM(prog);
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
				LastResult = lastPopped;
				OnNewResult?.Invoke(LastResult.ToString());
			}
			else
			{
				if (State == VMState.Complete)
				{
					_output.AppendLine("Finished.");
				}else if (State == VMState.Paused)
				{
					_output.AppendLine("--paused--");
				}
			}
			OnOutput?.Invoke();
		}
		Globals = _vm.Globals;//save for REPL oop.s
		
		
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

	public void Pause()
	{
		if (_vm != null)
		{
			// todo... if running
			if (_vm.State != VMState.Paused)
			{
				_vm.Pause();
			}
		}
	}
	
	public void RunNextExpression()
	{
		//uh... keep running until we hit some special 'expressionbreak' on the same frame we are on.
		//that's the best I got for now lol.
	}

	public void RunPrevExpression()
	{
		
	}
	public void RunNextOperation()
	{
		if (_vm == null)
		{
			return;
		}
		
		if (_vm.State == VMState.Paused)
		{
			_vm.RunOne();
			//todo: results.
		}
	}

	public void RunPreviousOperation()
	{
		if (_vm == null)
		{
			return;
		}

		if (_vm.State == VMState.Paused)
		{
			_vm.PreviousOne();
			//todo: results. to the history!
		}
	}

}