using scrub_lang.Parser;

namespace scrub_lang.Environment;

public class Environment
{
	//Keeps a list of execution passes and the current scope.
	//keeps the memory context.
	
	public void StepExecution(IExpression expression)
	{
		//add a tick of the execution
		//create a new execution step of with the expression and current context.
			
	}

	public void DescendExecution(IExpression expression)
	{
		
	}

	public void AscendExecution(IExpression blockExpression)
	{
		
	}
	
}