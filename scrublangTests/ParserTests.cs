using System.Text;
using NUnit.Framework.Internal;
using scrub_lang.Parser;
using scrub_lang.Tokenizer;

namespace scrublangTests;


public class Tests
{
	[SetUp]
	public void Setup()
	{
		//todo: properly refactor the old test code into c# styles.
	}

	[Test]
	public void ParsingTests()
	{
		Assert.IsTrue(ParseTest("a()", "a()"));
		ParseTest("a(b)", "a(b)");
		ParseTest("a(b, c)", "a(b, c)");
		ParseTest("a(b)(c)", "a(b)(c)");
		ParseTest("a(b) + c(d)", "(a(b) + c(d))");
		ParseTest("a(b ? c : d, e + f)", "a((b ? c : d), (e + f))");
		
		// Unary and binary binding power.
		ParseTest("-a * b", "((-a) * b)");
		ParseTest("!a + b", "((!a) + b)");
		ParseTest("~a ^ b", "((~a) ^ b)");
		ParseTest("-a--", "(-(a--))");
		ParseTest("-a++", "(-(a++))");
		
		
		//Binary Operator
		ParseTest("a == b", "(a == b)");
	}

	[Test]
	public void FunctionDeclareParse()
	{
		Assert.IsTrue(ParseTest("func (){}", "func (){\n}"));
		Assert.IsTrue(ParseTest("func a(){}", "func a(){\n}"));
		Assert.IsTrue(ParseTest("func a(b){a*a}", "func a(b){\n(a * a)\n}"));
		Assert.IsTrue(ParseTest("func a(){a()}", "func a(){\na()\n}"));
	}
	
	[Test]
	public void BlocksParse()
	{
		Assert.IsTrue(ParseTest("{}", "{\n}"));
		Assert.IsTrue(ParseTest("{};", "{\n}"));
		Assert.IsTrue(ParseTest("{a+b\nb++}", "{\n(a + b)\n(b++)\n}"));
	}
	
	[Test]
	public void BinaryBindingPower()
	{
		Assert.IsTrue(ParseTest("a = b + c * d ** e - f / g", "(a = ((b + (c * (d ** e))) - (f / g)))"));
	}
	
	[Test]
	public void BinaryAssociativityTest()
	{
		Assert.IsTrue(ParseTest("a = b = c", "(a = (b = c))"));
		Assert.IsTrue(ParseTest("a + b - c", "((a + b) - c)"));
		Assert.IsTrue(ParseTest("a * b / c", "((a * b) / c)"));
		Assert.IsTrue(ParseTest("a ** b ** c", "(a ** (b ** c))"));
	}
	
	[Test]
	public void TernaryTesting()
	{
		// Tenary operator.
		Assert.IsTrue(ParseTest("a ? b : c ? d : e", "(a ? b : (c ? d : e))"));
		Assert.IsTrue(ParseTest("a ? b ? c : d : e", "(a ? (b ? c : d) : e)"));
		Assert.IsTrue(ParseTest("a + b ? c * d : e / f", "((a + b) ? (c * d) : (e / f))"));
	}
	
	[Test]
	public void GroupingParsing()
	{
		Assert.IsTrue(ParseTest("a + (b + c) + d", "((a + (b + c)) + d)"));
		Assert.IsTrue(ParseTest("a ^ (b + c)", "(a ^ (b + c))"));
		Assert.IsTrue(ParseTest("(!a)++", "((!a)++)"));
	}
	
	[Test]
	public void UnaryBindingPowerParsing()
	{
		ParseTest("~!-+a", "(~(!(-(+a))))");
		ParseTest("a++++++", "(((a++)++)++)");
	}
	
	private bool ParseTest(string source, string expected)
	{
		var t = new Tokenizer(source);
		var parser = new Parser(t);

		try
		{
			var result = parser.ParseProgram();
			var builder = new StringBuilder();
			result.Print(builder);
			var actual = builder.ToString();
			if (actual != expected)
			{
				Assert.Fail($"actual: {actual} not equal to expected: {expected}");
				return false;
			}
			else
			{
				return true;
			}
		}
		catch (scrub_lang.Parser.ParseException ex)
		{
			Assert.Fail(ex.Message);
		}

		return false;
	}
}