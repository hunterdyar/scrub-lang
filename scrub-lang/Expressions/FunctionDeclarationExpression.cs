using System.Text;

namespace scrub_lang.Parser;

public class FunctionDeclarationExpression : IExpression
{
	private IdentifierExpression id;
	private List<IdentifierExpression> args;
	private IExpression block;

	public FunctionDeclarationExpression(IdentifierExpression id, List<IdentifierExpression> args, IExpression block)
	{
		this.id = id;
		this.args = args;
		this.block = block;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append("func ");
		id.Print(sb);
		sb.Append("(");
		for (int i = 0; i < args.Count; i++)
		{
			args[i].Print(sb);
			if (i < args.Count - 1)
			{
				sb.Append(",");
			}
		}

		sb.Append(")");
		block.Print(sb);
	}
}