using System.Text;

namespace scrub_lang.Parser;

public class ImportExpression : IExpression
{
	public Location Location { get; }

	public IExpression Import => _import;
	private IExpression _import;

	public ImportExpression(IExpression import, Location location)
	{
		this._import = import;
		Location = location;
	}
	public void Print(StringBuilder sb)
	{
		sb.Append("import ");
		_import.Print(sb);
	}
}