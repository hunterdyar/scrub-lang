using System.Text;

namespace scrub_lang.Parser;

public class NumberExpression : IExpression
{
	private readonly string _literal;
	private bool _isInt;
	private int _litInt;
	private bool _isFloat;
	private float _litFloat;

	public NumberExpression(string literal)
	{
		_isInt = int.TryParse(literal, out _litInt);
		_isFloat = float.TryParse(literal, out _litFloat);
		this._literal = literal;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append(_literal);
	}
}