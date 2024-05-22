using System.Diagnostics;
using System.Text;
using scrub_lang.Compiler;
using scrub_lang.Objects;
using scrub_lang.Parser;
using scrub_lang.VirtualMachine;

namespace scrub_lang;

public class Tests
{
	private static int _passed = 0;
	private static int _failed = 0;
	
	public static void Test()
	{
		//todo: REPL instead of testsuite.
		
		
		Stopwatch sw = new Stopwatch();
		sw.Start();
		Console.WriteLine("Parsing Tests...");
		TestParse();
		Console.WriteLine("Compiler Tests...");
		TestCompile();
		//VM tests...
		VMTests.TestIntegerArithmetic();
		
		TimeSpan ts = sw.Elapsed;
		var elapsedTime = $"{ts.Seconds}s; {ts.Microseconds} Microseconds. {ts.Ticks} ticks.";
		Console.WriteLine($"---\nCompleted in {elapsedTime}");
	}
	public static void TestParse()
	{
		_failed = 0;
		_passed = 0;
		// Function call.
		ParseTest("a()", "a()");
		ParseTest("a(b)", "a(b)");
		ParseTest("a(b, c)", "a(b, c)");
		ParseTest("a(b)(c)", "a(b)(c)");
		ParseTest("a(b) + c(d)", "(a(b) + c(d))");
		ParseTest("a(b ? c : d, e + f)", "a((b ? c : d), (e + f))");

		// Unary binding power.
		ParseTest("~!-+a", "(~(!(-(+a))))");
		ParseTest("a++++++", "(((a++)++)++)");

		// Unary and binary binding power.
		ParseTest("-a * b", "((-a) * b)");
		ParseTest("!a + b", "((!a) + b)");
		ParseTest("~a ^ b", "((~a) ^ b)");
		ParseTest("-a--", "(-(a--))");
		ParseTest("-a++", "(-(a++))");

		// Binary binding power.
		ParseTest("a = b + c * d ** e - f / g", "(a = ((b + (c * (d ** e))) - (f / g)))");

		// Function Declare
		ParseTest("func a(){}", "func a(){\n}");
		ParseTest("func a(b){a*a}","func a(b){\n(a * a)\n}");
		ParseTest("func a(){a()}", "func a(){\na()\n}");
		
		// Binary associativity.
		ParseTest("a = b = c", "(a = (b = c))");
		ParseTest("a + b - c", "((a + b) - c)");
		ParseTest("a * b / c", "((a * b) / c)");
		ParseTest("a ** b ** c", "(a ** (b ** c))");

		// Tenary operator.
		ParseTest("a ? b : c ? d : e", "(a ? b : (c ? d : e))");
		ParseTest("a ? b ? c : d : e", "(a ? (b ? c : d) : e)");
		ParseTest("a + b ? c * d : e / f", "((a + b) ? (c * d) : (e / f))");
		
		//Binary Operator
		ParseTest("a == b", "(a == b)");
		// Grouping.
		ParseTest("a + (b + c) + d", "((a + (b + c)) + d)");
		ParseTest("a ^ (b + c)", "(a ^ (b + c))");
		ParseTest("(!a)++", "((!a)++)");

		
		//Blocks
		ParseTest("{}","{\n}");
		ParseTest("{a+b\nb++}","{\n(a + b)\n(b++)\n}");

		if (_failed != 0) Console.WriteLine("----");
		Console.WriteLine("Passed: " + _passed);
		Console.WriteLine("Failed: " + _failed);
	}
	public static void TestCompile()
	{
		_failed = 0;
		_passed = 0;
		
		//0 and 1 are the locations in the constants pool.
		CompileTest("1 + 2", [new Integer(1),new Integer(2)], Op.Make(OpCode.OpConstant, 0), Op.Make(OpCode.OpConstant, 1), Op.Make(OpCode.OpAdd), Op.Make(OpCode.OpPop));


		if (_failed != 0) Console.WriteLine("----");
		Console.WriteLine("Passed: " + _passed);
		Console.WriteLine("Failed: " + _failed);
	}

	public static void CompileTest(string input, object[] expectedConstants, params byte[][] expectedInstructions)
	{
		var expInstructions = Op.ConcatInstructions(expectedInstructions);
	
		try
		{
			var t = new Tokenizer.Tokenizer(input);
			var p = new Parser.Parser(t);
			var ast = p.ParseProgram();
			var c = new Compiler.Compiler();
			var e = c.Compile(ast);
			if (e != null)
			{
				_failed++;
				Console.WriteLine("[FAIL] Source: " + input);
				Console.WriteLine("       CompileError: " + e);
				return;
			}

			var byteCode = c.ByteCode();
			bool failed = false;

			//todo: fix constants with our own objects. I think the easier fix is to allow comparison of our objects and native versions with equality.
			// if (!expectedConstants.SequenceEqual(byteCode.Constants))
			// {
			// 	Console.WriteLine("[FAIL] Source: " + input);
			// 	Console.WriteLine("       Expected Constants:\n " + expectedConstants.ToDelimitedString());
			// 	Console.WriteLine("       Actual Constants:\n " + byteCode.Constants.ToDelimitedString());
			// 	failed = true;
			// }

			if (!expInstructions.SequenceEqual(byteCode.Instructions))
			{
				if (!failed)
				{
					Console.WriteLine("[FAIL] Source: " + input);
					failed = true;
				}

				Console.WriteLine("       Expected Instructions:\n " + Op.InstructionsToString(expInstructions));
				Console.WriteLine("       Actual Instructions:\n " + Op.InstructionsToString(byteCode.Instructions));
			}

			_failed += failed ? 1 : 0;
			_passed += failed ? 0 : 1;
		}catch (CompileException cx)
		{
			_failed++;
			Console.WriteLine("[FAIL] Source: " + input);
			Console.WriteLine("        Error: " + cx.Message);
		}
	}
	public static void ParseTest(string source, string expected)
	{
		var t = new Tokenizer.Tokenizer(source);
		var parser = new Parser.Parser(t);

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