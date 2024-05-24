using Microsoft.VisualBasic;
using scrub_lang.Compiler;
using scrub_lang.Objects;

namespace scrub_lang.VirtualMachine;

public static class VMTests
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
		}
	}

	public static void RunAllVMTests()
	{
		TestIntegerArithmetic();
		TestGlobalAssignStatements();
	}
	public static void TestIntegerArithmetic()
	{
		Console.WriteLine("Running VM Arthithmetic Tests...");
		Failures = 0;
		Tests = new List<VMTestCase>()
		{
			new VMTestCase("1", new Integer(1)),
			new VMTestCase("2", new Integer(2)),
			new VMTestCase("1+2", new Integer(3)),
			new VMTestCase("a = func(a){a+2}", VM.Null),// just writing this failing case down for now.
			new VMTestCase("func a(){};a()", VM.Null)
		};
		RunTests();
		Console.WriteLine($"Failures: {Failures}");

	}

	public static void TestGlobalAssignStatements()
	{
		Console.WriteLine("Running VM Global Assignment Tests...");
		Failures = 0;
		Tests = new List<VMTestCase>()
		{
			new VMTestCase("one = 1\n one", new Integer(1)),
			new VMTestCase("one = 1\n two = 2\n one + two", new Integer(3)),
			new VMTestCase("one = 1\ntwo = one+one\n one+two", new Integer(3))
		};
		RunTests();
		Console.WriteLine($"Failures: {Failures}");

	}

	
	public static void RunTests()
	{
		foreach (var test in Tests)
		{
			var p = VM.Parse(test.input);
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
			
			//todo: compare the stacks
			var top = vm.LastPopped();
			CompareObjects(test.expected, top);
		}
	}

	static bool CompareObjects(object expected, object actual)
	{
		if (actual == null)
		{
			throw new VMException($"Test Failure. Expected {expected}, got null");
			//todo: this won't catch if we want null... low priority.
		}

		if (expected is Null n)
		{
			if (!(actual is Null))
			{
				throw new VMException($"test failure. Object not null: {actual}");
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
					Failures++;
					throw new VMException($"Test Failure. e: {i} is not a: {a}.");
				}

				return r;
			}

			Failures++;
			throw new VMException($"Test Failure. e: {i} is not a: {actual.ToString()}.");
		}

		Failures++;
		throw new VMException($"Test Failure. e: {expected} is not a: {actual.ToString()}.");
	}
}