using System.Text;
using scrub_lang.Compiler;
using scrub_lang.Evaluator;
using scrub_lang.Parser;

namespace scrub_lang.Evaluator;

public class Environment
{
	public SymbolTable SymbolTable;
	public List<Objects.Object> Constants;
}