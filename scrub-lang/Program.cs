
using System.Diagnostics;
using System.Text;
using scrub_lang.Evaluator;
using scrub_lang.Parser;
using scrub_lang.Tokenizer;

static class Scrub
{
	private static int _passed = 0;
	private static int _failed = 0;

	public static void Main()
	{
		var t = new Tokenizer("2+2*3");
		var parser = new Parser(t);
		var program = parser.ParseProgram();
		var sb = new StringBuilder();
		program.Print(sb);
		Console.WriteLine(sb);
		var evaluator = new Evaluator(program);
		Console.WriteLine("---eval");
		evaluator.Evaluate();
		Console.WriteLine("---env");
		evaluator.Environment.PrintExecution();
	}
	public static void TestParse()
	{
		// Function call.
		Test("a()", "a()");
		Test("a(b)", "a(b)");
		Test("a(b, c)", "a(b, c)");
		Test("a(b)(c)", "a(b)(c)");
		Test("a(b) + c(d)", "(a(b) + c(d))");
		Test("a(b ? c : d, e + f)", "a((b ? c : d), (e + f))");

		// Unary binding power.
		Test("~!-+a", "(~(!(-(+a))))");
		Test("a++++++", "(((a++)++)++)");

		// Unary and binary binding power.
		Test("-a * b", "((-a) * b)");
		Test("!a + b", "((!a) + b)");
		Test("~a ^ b", "((~a) ^ b)");
		Test("-a--", "(-(a--))");
		Test("-a++", "(-(a++))");

		// Binary binding power.
		Test("a = b + c * d ** e - f / g", "(a = ((b + (c * (d ** e))) - (f / g)))");

		// Function Declare
		Test("func a(){}", "func a(){\n}");
		Test("func a(b){a*a}","func a(b){\n(a * a)\n}");
		Test("func a(){a()}", "func a(){\na()\n}");
		
		// Binary associativity.
		Test("a = b = c", "(a = (b = c))");
		Test("a + b - c", "((a + b) - c)");
		Test("a * b / c", "((a * b) / c)");
		Test("a ** b ** c", "(a ** (b ** c))");

		// Tenary operator.
		Test("a ? b : c ? d : e", "(a ? b : (c ? d : e))");
		Test("a ? b ? c : d : e", "(a ? (b ? c : d) : e)");
		Test("a + b ? c * d : e / f", "((a + b) ? (c * d) : (e / f))");
		
		//Binary Operator
		Test("a == b", "(a == b)");
		// Grouping.
		Test("a + (b + c) + d", "((a + (b + c)) + d)");
		Test("a ^ (b + c)", "(a ^ (b + c))");
		Test("(!a)++", "((!a)++)");

		
		//Blocks
		Test("{}","{\n}");
		Test("{a+b\nb++}","{\n(a + b)\n(b++)\n}");

		if (_failed != 0) Console.WriteLine("----");
		Console.WriteLine("Passed: " + _passed);
		Console.WriteLine("Failed: " + _failed);
	}

	public static void Test(string source, string expected)
	{
		var t = new Tokenizer(source);
		var parser = new Parser(t);

		try
		{
			var result = parser.ParseProgram();
			var builder = new StringBuilder();
			result.Print(builder);
			var actual = builder.ToString();

			if (expected.Equals(actual))
			{
				_passed++;
			}
			else
			{
				_failed++;
				Console.WriteLine("[FAIL] Source: " + source);
				Console.WriteLine("     Expected: " + expected);
				Console.WriteLine("       Actual: " + actual);
			}
		}
		catch (ParseException ex)
		{
			_failed++;
			Console.WriteLine("[FAIL] Source: " + source);
			Console.WriteLine("     Expected: " + expected);
			Console.WriteLine("        Error: " + ex.Message);
		}
	}
}