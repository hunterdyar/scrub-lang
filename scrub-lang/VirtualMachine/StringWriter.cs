using System.Text;

namespace scrub_lang.VirtualMachine;

public class StringWriter : TextWriter
{
	public override Encoding Encoding => Encoding.UTF8;

	public Action OnUpdate;
	private StringBuilder _sb;
	
	public StringWriter()
	{
		_sb = new StringBuilder();
	}

	public override void Write(string? value)
	{
		_sb.Append(value);
		OnUpdate?.Invoke();
	}

	public override void WriteLine(string? value)
	{
		_sb.AppendLine(value);
		OnUpdate?.Invoke();
	}

	public override string ToString()
	{
		return _sb.ToString();
	}
}