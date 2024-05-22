using System.Text;

namespace scrub_lang.Parser;

public class FunctionDeclarationExpression : IExpression
{
	public IdentifierExpression Identity => _id;
	private readonly IdentifierExpression _id;
	public List<IdentifierExpression> Arguments => _args;//todo: wait are these "arguments" or "parameters"?
	private readonly List<IdentifierExpression> _args;
	public IExpression Expression => _block;
	private readonly IExpression _block;

	public FunctionDeclarationExpression(IdentifierExpression id, List<IdentifierExpression> args, IExpression block)
	{
		this._id = id;
		this._args = args;
		this._block = block;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append("func ");
		_id.Print(sb);
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