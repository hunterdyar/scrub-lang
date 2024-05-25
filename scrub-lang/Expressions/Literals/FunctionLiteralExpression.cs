using System.Text;

namespace scrub_lang.Parser;

public class FunctionLiteralExpression : IExpression
{
	public List<IdentifierExpression> Arguments => _args; //todo: wait are these "arguments" or "parameters"?
	private readonly List<IdentifierExpression> _args;
	public IExpression Expression => _block;
	private readonly IExpression _block;
	public string Name;
	public Location Location { get; }

	public FunctionLiteralExpression(List<IdentifierExpression> args, IExpression block, Location location, string funcName = "")
	{
		Location = location;
		Name = funcName;
		this._args = args;
		this._block = block;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append("func ");
		sb.Append("(");
		for (int i = 0; i < _args.Count; i++)
		{
			_args[i].Print(sb);
			if (i < _args.Count - 1)
			{
				sb.Append(",");
			}
		}

		sb.Append(")");
		_block.Print(sb);
	}
}