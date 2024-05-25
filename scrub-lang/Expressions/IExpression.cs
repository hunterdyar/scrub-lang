using System.Text;
using scrub_lang.Tokenizer;

namespace scrub_lang.Parser;

public interface IExpression
{
	public Location Location { get; }
	void Print(StringBuilder sb);

	bool ReturnsValue => true;
}