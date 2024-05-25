using Microsoft.VisualBasic;
using scrub_lang.Compiler;
using scrub_lang.Objects;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.VirtualMachine;

public class VMTests
{
	public static List<VMTestCase> Tests = new List<VMTestCase>();
	public static int Failures;
	//VMTestCase struct
	public struct VMTestCase
	{
		public string input;
		public object expected;

		public VMTestCase(string input,object expected)
		{
			this.expected = expected;
			this.input = input;
			TestCase();
		}

		public bool TestCase()
		{
			var p = VM.Parse(input);
			var comp = new Compiler.Compiler();

			var error = comp.Compile(p);
			if (error != null)
			{
				//testException
				Failures++;
				throw new VMException(error.ToString());
			}

			var vm = new VM(comp.ByteCode());

			var vmerror = vm.Run();
			if (vmerror != null)
			{
				Failures++;
				throw new VMException(vmerror.ToString());
			}

			var top = vm.LastPopped();
			return CompareObjects(expected, top);
		}
	}
	
	[Test]
	public void TestIntegerArithmetic()
	{
		new VMTestCase("1", new Integer(1));
		new VMTestCase("2", new Integer(2));
		new VMTestCase("1+2", new Integer(3));
	
	}

	[Test]
	public void TestGlobalAssignStatements()
	{
		new VMTestCase("one = 1", new Integer(1));
		new VMTestCase("one = 1\n one", new Integer(1));
		new VMTestCase("one = 1\n two = 2\n one + two", new Integer(3));
		new VMTestCase("one = 1\ntwo = one+one\n one+two", new Integer(3));
	}

	[Test]
	public void TestFunctionDec()
	{
		//todo: i am unable to compare closures (closures, compiled objects) correctly.
		//new VMTestCase("func (){}", new Closure(new Function([(byte)OpCode.OpNull],0),null));
		new VMTestCase("a = func(b){b+2};a(1)", new Integer(3));
		new VMTestCase("func a(){};a()", VM.Null);
		new VMTestCase("a = func b(){12};a()", new Integer(12));
		new VMTestCase("b = func(a){a+1};c = b;c(2)", new Integer(3));
		new VMTestCase("b = func(a){a()};c = b;c(func(){3})", new Integer(3));

	}

	[Test]
	public void TestRescursiveCallFib()
	{
		string fib = """
		              func fib(x){
		              if (x==0){return 0}
		              if (x==1){return 1}
		              return (fib(x-1)+fib(x-2))
		              }
		              """;
		new VMTestCase(fib + "fib(0)", new Integer(0));
		new VMTestCase(fib + "fib(2)", new Integer(1));
		new VMTestCase(fib + "fib(9)", new Integer(34));
		//above works but below fails?
		new VMTestCase(fib + "fib(10)", new Integer(55));
		new VMTestCase(fib + "fib(11)", new Integer(89));
		new VMTestCase(fib + "fib(20)", new Integer(6765));

	}
	static bool CompareObjects(object expected, object actual)
	{
		if (actual == null)
		{
			Assert.Fail($"expected {expected}, got native null");
			return false;
		}

		if (expected is Null n)
		{
			if (!(actual is Null))
			{
				Assert.Fail($"Object not null: {actual}");
				return false;
			}//else
			return true;
		}
		if (expected is Integer i)
		{
			if (actual is Integer a)
			{
				bool r = i.NativeInt == a.NativeInt;
				if (!r)
				{
					Assert.Fail($"expected: {i}, got {a}.");
					return false;
				}

				return r;
			}
			Assert.Fail($"e: {i} is not a: {actual.ToString()}.");
		}

		if (expected == actual)
		{
			return true;
		}

		var eo = (Object)expected;
		var ao = (Object)actual;
		//
		if (eo.GetType() != ao.GetType())
		{
			Assert.Fail($" expected type {eo.GetType()}. Got {ao.GetType()}. e: {eo} a: {ao.ToString()}.");
			return false;
		}else if(eo.Bits.Xor(ao.Bits).OfType<bool>().All(e => !e)) //handle casts  )
		{
			Assert.Fail($" expected {eo.GetType()}. Expected {eo} is not a: {ao.ToString()}.");
			return false;
		}

		return true;
	}
}