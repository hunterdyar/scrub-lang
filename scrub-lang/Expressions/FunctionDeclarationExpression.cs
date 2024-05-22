using System.Text;

namespace scrub_lang.Parser;

//todo; should probably have function literal and function declaration (assign function literal to some identifier) as separate expressions. 
public class FunctionDeclarationExpression : IExpression
{
	public List<IdentifierExpression> Arguments => _args;//todo: wait are these "arguments" or "parameters"?
	private readonly List<IdentifierExpression> _args;
	public IExpression Expression => _block;
	private readonly IExpression _block;

	public FunctionDeclarationExpression( List<IdentifierExpression> args, IExpression block)
	{
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