using System.Text;

namespace scrub_lang.Parser;

public class ArrayLookupExpression : IExpression
{
	public IdentifierExpression Identity => _identity;
	private IdentifierExpression _identity;
	public IExpression Index => _index;
	private IExpression _index;

	public ArrayLookupExpression(IdentifierExpression identity, IExpression index)
	{
		_identity = identity;
		_index = index;
	}

	public void Print(StringBuilder sb)
	{
		_identity.Print(sb);
		sb.Append('[');
		_index.Print(sb);
		sb.Append(']');
	}
}