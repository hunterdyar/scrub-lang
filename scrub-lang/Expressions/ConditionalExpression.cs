using System.Text;

namespace scrub_lang.Parser;

/// <summary>
/// a ? b : c
/// or if(a){b}else{c}
/// </summary>
public class ConditionalExpression : IExpression
{
	public IExpression Conditional => _conditionExpr;
	private readonly IExpression _conditionExpr;
	public IExpression Consequence => _consequenceExpr;
	private readonly IExpression _consequenceExpr;
	public IExpression? Alternative =>_alterativeExpr;
	private readonly IExpression? _alterativeExpr;
	public Location Location { get; }

	public ConditionalExpression(IExpression conditionExpr, IExpression consequenceExpr, IExpression? aterativeExpr, Location location)
	{
		Location = location;
		_conditionExpr = conditionExpr;
		_consequenceExpr = consequenceExpr;
		_alterativeExpr = aterativeExpr;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('(');
		_conditionExpr.Print(sb);
		sb.Append(" ? ");
		_consequenceExpr.Print(sb);
		if (_alterativeExpr != null)
		{
			sb.Append(" : ");
			_alterativeExpr.Print(sb);
		}

		sb.Append(')');
	}
}