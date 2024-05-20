using System.Text;
using scrub_lang.Evaluator;
using scrub_lang.Parser;

namespace scrub_lang.Evaluator;

public class Environment
{
	//Keeps a list of execution passes and the current scope.
	private List<ExecutionStep> _executionSteps = new List<ExecutionStep>();

	private int currentScope = 0;
	//keeps the memory context.
	
	public void StepExecution(IExpression expression, Result result)
	{
		var step = new ExecutionStep(currentScope,expression, result);
		_executionSteps.Add(step);
		//add a tick of the execution
		//create a new execution step of with the expression and current context.
	}

	public void StepExecution(IExpression expression, Result result, string message)
	{
		var step = new ExecutionStep(currentScope, message, expression, result);
		_executionSteps.Add(step);
		//add a tick of the execution
		//create a new execution step of with the expression and current context.
	}

	public void PrintExecution()
	{
		StringBuilder sb = new StringBuilder();
		foreach (var step in _executionSteps)
		{
			step.Print(sb);
		}

		Console.Write(sb.ToString());
	}

	public void Descend(Result res)
	{
		_executionSteps.Add(new ExecutionStep(currentScope,res));
		currentScope--;
	}

	public void Ascend(string ascendMessage)
	{
		_executionSteps.Add(new ExecutionStep(currentScope,ascendMessage));
		currentScope++;
	}
	
}