using System.Text;
using scrub_lang.Parser;

namespace scrub_lang.Environment;

public class ExecutionStep
{
	// private IMemoryContext 
	private IExpression _expression;
	// private Memory _memory;
	
	private int _scope;
	public ExecutionStep(int scope, IExpression expression)
	{
		_expression = expression;
		_scope = scope;
	}

	public void Print(StringBuilder sb)
	{
		//
		for (int i = 0; i < _scope; i++)
		{
			sb.Append("- ");
		}
		_expression.Print(sb);
		sb.Append("\n");
	}
}