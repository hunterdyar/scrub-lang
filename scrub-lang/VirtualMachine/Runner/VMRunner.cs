using System.Diagnostics;
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
	public Action<string, TimeSpan> OnNewResult;
	public Action OnInitialized;//called when there is a new VM object created.
	//core references
	public Status Status => _status;
	private Status _status;
	private VM? _vm;
	private Environment _environment;
	public StringWriter Output => _output;
	private StringWriter _output = new StringWriter();
	private Object[] Globals;
	//convenience
	public VMState? State => _vm?.State;
	public Object? LastResult;
	public TimeSpan LastExecutionTime => new TimeSpan(_executionStopWatch.ElapsedTicks);
	public float Percentage => _vm != null ? _vm.Progress.Percentage : 0;
	public ExecutionLog.ExecutionLog Log => _vm != null ? _vm.Log : null;
	
	private Stopwatch _executionStopWatch = new Stopwatch();
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
			_output.WriteLine("Parse Error: " + pe.Message);
			return;
		}
		var compiler = new Compiler.Compiler(env);
		if (!compiler.TryCompile(root, out var error, out var prog))
		{
			_output.WriteLine("Compiler Error: " + error.Message);
			return;
		}
		_vm = new VM(prog, _output);
		_environment = compiler.Environment();
		_status = new Status(_vm);
		OnInitialized?.Invoke();
	}

	public void CompileProgram(StreamReader program)
	{
		IExpression root;
		try
		{
			root = Scrub.Parse(program);
		}
		catch (ParseException pe)
		{
			_output.WriteLine("Parse Error: " + pe.Message);
			return;
		}

		var compiler = new Compiler.Compiler();
		if (!compiler.TryCompile(root, out var error, out var prog))
		{
			_output.WriteLine("Compiler Error: " + error.Message);
			return;
		}

		_vm = new VM(prog, _output);
		_environment = compiler.Environment();
		_status = new Status(_vm);
		OnInitialized?.Invoke();
	}

	public void RunWithEnvironment(string program)
	{
		_executionStopWatch.Restart();
		CompileProgram(program, _environment);
		if (_vm != null){
			_vm.SetGlobals(Globals);
			RunUntilStop();
		}

		_executionStopWatch.Stop();
	}
	
	/// <summary>
	/// Compiles and then Runs.
	/// </summary>
	public void Run(string program)
	{
		_executionStopWatch.Restart();
		CompileProgram(program);
		if (_vm != null)
		{
			RunUntilStop();
		}
		_executionStopWatch.Stop();
	}

	public void Run(StreamReader program)
	{
		_output.Clear();
		_executionStopWatch.Restart();
		CompileProgram(program);
		if (_vm != null)
		{
			RunUntilStop();
		}
		_executionStopWatch.Stop();
	}

	public void RunUntilStop()
	{
		if (_vm == null)
		{
			_output.WriteLine("A program must be compiled before it can be run.");
			return;
		}
		var res = _vm.Run();
		if (res != null)
		{
			//report.
			_output.WriteLine(res.Message);
		}
		else
		{
			var lastPopped = _vm.LastPopped();
			if (lastPopped != null)
			{
				_output.WriteLine(lastPopped.ToString());
				LastResult = lastPopped;
				OnNewResult?.Invoke(LastResult.ToString(),LastExecutionTime);
			}
			else
			{
				if (State == VMState.Complete)
				{
					_output.WriteLine("Finished.");
				}else if (State == VMState.Paused)
				{
					_output.WriteLine("--paused--");
				}
			}
		}
		Globals = _vm.Globals;//save for REPL oop.
		
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
			OnPaused?.Invoke();
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
			OnPaused?.Invoke();
		}
	}

	public void RunTo(int logOpNumber)
	{
		if (_vm != null)
		{
			_vm.RunTo(logOpNumber);
			//todo: check state.
			OnPaused?.Invoke();
		}
	}
}