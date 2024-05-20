using System.Linq.Expressions;
using System.Text;
using scrub_lang.Evaluator.Evaluators;
using scrub_lang.Memory;
using scrub_lang.Parser;

namespace scrub_lang.Evaluator;

//The evaluator takes a single expression, usually a ProgramExpression, and evaluates it.
public class Evaluator
{
	private IExpression _root;
	public Environment Environment => _environment;//i've done something wrong.
	private Environment _environment;
	public Evaluator(IExpression root)
	{
		_root = root;
		//_environment = new Environment.Environment();
	}


	public void Evaluate()
	{
		//clear everything and set.
		//.Clear
		_environment = new Environment();
		//clear root and set.
		var result = Eval(_root, _environment);
		if (result.HasError)//error
		{
			//change syntax
			Console.Error.WriteLine(result);
		}
		else
		{
			Console.WriteLine(result);
		}
	}
	//todo: make a Result Type
	public Result Eval(IExpression expression, Environment environment)
	{
		if (expression == null)
		{
			return new Result(new ScrubRuntimeError("Unable to Evaluate null?", expression));
		}
		if (expression is ProgramExpression program)
		{
			Result res = new Result();
			environment.Ascend("Program");
			foreach (var e in program.Expressions)
			{
				//step? Pass memory context along.
				//return the last value.
				res = Eval(e,environment);
			}
			environment.Descend(res);

			if (!res.HasObject)
			{
				return new Result(res.ScrubObject, new ScrubRuntimeError("No Expression in Program!"));
			}
			
			return res;
		}else if (expression is ExpressionGroupExpression expressionGroupExpression)
		{
			Result res = new Result();
			environment.Ascend("{}");
			foreach (var e in expressionGroupExpression.Expressions)
			{
				//step? Pass memory context along.
				//return the last value.
				res = Eval(e,environment);
			}
			environment.Descend(res);
			if (res.HasError)
			{
				return res;
			}
			
			if (!res.HasObject)//and no object... this is basically if expressions.count == 0.
			{
				res = new Result(res.ScrubObject, new ScrubRuntimeError("No Expression in Expression Group!"));
				environment.StepExecution(expression,res);
				return res;
			}

			return res;
		}else if (expression is BoolLiteralExpression ble)
		{
			return new (new ScrubObject(ble.Literal), null);
		}else if (expression is NumberLiteralExpression nle)
		{
			if (nle.IsDouble)
			{
				return new Result(new ScrubObject(nle.AsDouble), null);
			}else if (nle.IsInt)
			{
				return new Result(new ScrubObject(nle.AsInt), null);
			}
			else
			{
				return new (null, new ScrubRuntimeError($"What type of number is {nle.Literal} ?"));
			}
		}else if (expression is BinaryMathExpression bme)
		{
			var result = MathEvaluator.Evaluate(this, bme, environment);
			return result;
		}

		StringBuilder sb = new StringBuilder();
		expression.Print(sb);
		return new (null, new ScrubRuntimeError($"Unable to Evaluate {sb}"));
	}
}