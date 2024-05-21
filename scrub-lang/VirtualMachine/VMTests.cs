using scrub_lang.Compiler;

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

	public static void TestIntegerArithmetic()
	{
		Console.WriteLine("Running VM Arthithmetic Tests...");
		Failures = 0;
		Tests = new List<VMTestCase>()
		{
			new VMTestCase("1", 1),
			new VMTestCase("2", 2),
			new VMTestCase("1+2", 3)
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
			var top = vm.StackTop();
			CompareObjects(test.expected, top);
		}
	}

	static bool CompareObjects(object expected, object actual)
	{
		if (expected is int i)
		{
			if (int.TryParse(actual.ToString(), out var a))
			{
				bool r = (Int64)i == (Int64)a;
				if (!r)
				{
					Failures++;
					throw new VMException($"Test Failure. e: {i} is not a: {a}.");
				}

				return r;
			}

			Failures++;
			throw new VMException($"Test Failure. e: {i} is not a: {a.ToString()}.");
		}

		Failures++;
		throw new VMException($"Test Failure. e: {expected} is not a: {actual.ToString()}.");
	}
}