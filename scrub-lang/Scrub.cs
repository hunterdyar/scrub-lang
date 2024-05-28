using scrub_lang.Compiler;
using scrub_lang.Parser;

namespace scrub_lang;

public static class Scrub
{
	//todo: move these out of VM class.
	public static IExpression Parse(string input)
	{
		var lexer  = new Tokenizer.Tokenizer(input);
		var parser = new Parser.Parser(lexer);
		return parser.ParseProgram();
	}

	public static IExpression Parse(StreamReader input)
	{
		var lexer = new Tokenizer.Tokenizer(input);
		var parser = new Parser.Parser(lexer);
		return parser.ParseProgram();
	}

	public static Program Compile(IExpression program)
	{
		Compiler.Compiler c = new Compiler.Compiler();
		c.Compile(program);
		return c.GetProgram();
	}

}