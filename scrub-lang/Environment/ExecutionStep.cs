using System.Text;
using scrub_lang.Evaluator;
using scrub_lang.Parser;

namespace scrub_lang.Evaluator;

public class ExecutionStep
{
	// private IMemoryContext 
	private IExpression _expression;
	private string _message;
	private Result _result;
	// private Memory _memory;
	
	private int _scope;
	public ExecutionStep(int scope, string message, IExpression expression, Result result)
	{
		_message = message;
		_expression = expression;
		_scope = scope;
		_result = result;
	}

	public ExecutionStep(int scope, string message)
	{
		_message = message;
		_expression = null;
		_scope = scope;
		_result = null;
	}

	public ExecutionStep(int scope, Result result)
	{
		_message = "";
		_expression = null;
		_scope = scope;
		_result = result;
	}

	public ExecutionStep(int scope, IExpression expression, Result result)
	{
		StringBuilder sb = new StringBuilder();
		expression.Print(sb);
		_message = "sb.ToString()";
		_expression = expression;
		_scope = scope;
		_result = result;
	}

	public void Print(StringBuilder sb)
	{
		//
		for (int i = 0; i < _scope; i++)
		{
			sb.Append("- ");
		}

		//if error
		if (_result != null && _result.HasError)
		{
			sb.Append(_result.Error);
			return;
		}
		
		//if success
		sb.Append(_message);
		if (_result != null)
		{
			if (_expression != null)
			{
				sb.Append(" -> ");
				sb.Append(_result);
			}
			else
			{
				sb.Append("<- ");
				sb.Append(_result);
			}
		}

		sb.Append("\n");
	}
}