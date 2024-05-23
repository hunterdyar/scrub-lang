using System.Text;

namespace scrub_lang.Parser;

public class FunctionDeclarationExpression : IExpression
{
	public IdentifierExpression Identity => _identifierExpression;
	private IdentifierExpression _identifierExpression;
	public FunctionLiteralExpression Function => _functionLiteral;
	private FunctionLiteralExpression _functionLiteral;
	public FunctionDeclarationExpression(IdentifierExpression ident, FunctionLiteralExpression function)
	{
		this._identifierExpression = ident;
		this._functionLiteral = function;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append("func ");
		
		_identifierExpression.Print(sb);
		sb.Append("(");
		for (int i = 0; i < _functionLiteral.Arguments.Count; i++)
		{
			_functionLiteral.Arguments[i].Print(sb);
			if (i < _functionLiteral.Arguments.Count - 1)
			{
				sb.Append(",");
			}
		}

		sb.Append(")");
		_functionLiteral.Expression.Print(sb);
	}
}