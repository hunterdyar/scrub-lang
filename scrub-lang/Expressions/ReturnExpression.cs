using System.Text;

namespace scrub_lang.Parser;

public class ReturnExpression : IExpression
{
	private bool returnVoid;
	private IExpression _retExpression;

	public ReturnExpression(IExpression returnExpression)
	{
		if (!(returnExpression is NullExpression))
		{
			returnVoid = false;
			_retExpression = returnExpression;
		}
		else
		{
			returnVoid = true;
		}
	}

	public void Print(StringBuilder sb)
	{
		sb.Append("return");
	}
}