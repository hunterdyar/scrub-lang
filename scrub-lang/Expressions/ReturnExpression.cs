using System.Text;
namespace scrub_lang.Parser;

public class ReturnExpression : IExpression
{
	public IExpression ReturnValue => _retExpression;
	private IExpression _retExpression;
	public Location Location { get; }

	public ReturnExpression(IExpression? returnExpression, Location location)
	{
		Location = location;
		//not sure if this ever happens.
		if (returnExpression == null)
		{
			_retExpression = new NullExpression(location);
		}
		else
		{
			_retExpression = returnExpression;
		}
	}

	public void Print(StringBuilder sb)
	{
		sb.Append("return");
	}
}