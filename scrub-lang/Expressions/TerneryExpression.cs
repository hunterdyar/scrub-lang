using System.Text;

namespace scrub_lang.Parser;

/// <summary>
/// a ? b : c
/// </summary>
public class TerneryExpression : IExpression
{
	private IExpression _conditionExpr;
	private IExpression _thenExpr;
	private IExpression _elseExpr;

	public TerneryExpression(IExpression conditionExpr, IExpression thenExpr, IExpression elseExpr)
	{
		_conditionExpr = conditionExpr;
		_thenExpr = thenExpr;
		_elseExpr = elseExpr;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('(');
		_conditionExpr.Print(sb);
		sb.Append(" ? ");
		_thenExpr.Print(sb);
		sb.Append(" : ");
		_elseExpr.Print(sb);
		sb.Append(')');
	}
}