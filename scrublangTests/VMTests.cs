using Microsoft.VisualBasic;
using scrub_lang.Compiler;
using scrub_lang.Objects;
using Object = scrub_lang.Objects.Object;
using String = scrub_lang.Objects.String;

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
				throw new VMException(error.ToString());
			}

			var vm = new VM(comp.ByteCode());

			var vmerror = vm.Run();
			if (vmerror != null)
			{
				throw new VMException(vmerror.ToString());
			}

			var top = vm.LastPopped();
			var b= CompareObjects(expected, top);
			Assert.IsTrue(b);
			return b;
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
	public void TestStrings()
	{
		new VMTestCase("\"hi\"", new String("hi"));
		
		//this failure is with string concat, not related to my current pop-pains
		//new VMTestCase("\"hi\"+\"hi\"", new String("hihi"));
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
		//new VMTestCase("func (){}", new Closure(new Function([(byte)OpCode.OpNull],0),null));
		new VMTestCase("a = func(b){b+2};a(1)", new Integer(3));
		new VMTestCase("func a(){};a()", VM.Null);
		new VMTestCase("a = func b(){12};a()", new Integer(12));
		new VMTestCase("b = func(a){a+1};c = b;c(2)", new Integer(3));
		//it's this one that's failing, not sure why yet.
		//new VMTestCase("b = func(a){a()};c = b;c(func(){3})", new Integer(3));
		new VMTestCase("func a(b){};a(1)", VM.Null);
		new VMTestCase("func a(b,c){};a(1,2)", VM.Null);
		new VMTestCase("func a(b,c,d){};a(1,2,3)", VM.Null);
		new VMTestCase("func a(b,c,d,e){};a(1,2,3,4)", VM.Null);

	}

	[Test]
	public void TestFunctionReturns()
	{
		//todo: i am unable to compare closures (closures, compiled objects) correctly.
		//new VMTestCase("func (){}", new Closure(new Function([(byte)OpCode.OpNull],0),null));
	//	new VMTestCase("a = func(b){return 1};a(500)", new Integer(1));
	//	new VMTestCase("func a(){return null};a()", VM.Null);
	//	new VMTestCase("a = func b(){0;return 12;0;0};a()", new Integer(12));
	//	new VMTestCase("b = func(a){101;return a+1;100};c = b;c(2)", new Integer(3));
	//	new VMTestCase("b = func(a){return a()};c = b;c(func(){3})", new Integer(3));
		new VMTestCase("""
		               func f(a){
		                   b = 0;
		                   if(a > 5)
		                   {
		                        b = b+1
		                   }else{
		                        b = b-1
		                   }
		                   return b
		               }
		               f(1)
		               """, new Integer(-1));
	}

	[Test]
	public void TestFunctionEarlyReturnOutOfConsequence()
	{
		//todo: i am unable to compare closures (closures, compiled objects) correctly.
		new VMTestCase("a = func(b){if(b == 1){return b}else{return 0}};a(1)", new Integer(1));
		new VMTestCase("a = func(b){134;if(b == 1){return 1+b}else{return 0};123;};a(1)", new Integer(2));
		new VMTestCase("func a(b){if(b == 1){20;30;40;return b;50;60;}else{return 0}};a(1)", new Integer(1));
		new VMTestCase("func a(){0;return 5;2;3;4;}a()", new Integer(5));
	}

	[Test]
	public void TestEarlyReturnOutOfAlternative()
	{
		new VMTestCase("a = func(){456;if(false){return 1}else{ \"hi\"} }; a()", new String("hi"));
		//new VMTestCase("a = func(b){456;if(b == 1){return b}else{return \"hi\"} 123;}; a(2)", new String("hi"));
	}

	[Test]
	public void TestLocalsPopped_2()
	{
		//I think, this and the above case, the return statement isn't getting pushed, so the top of the stack is the last local.
		//that me
		new VMTestCase("""
		               f = func(){
		               456;
		               if(false == 0){
		                 return "no"
		               }else{
		                 return "hi"
		               }
		                 123;
		               };

		               f()
		               """, new String("hi"));
		//new VMTestCase("a = func(b){if(b != 1){return b}else{1;2;3;return 0;4}};a(1)", new Integer(0));
	}
	[Test]
	public void TestLocalsPopped()
	{
		//I think, this and the above case, the return statement isn't getting pushed, so the top of the stack is the last local.
		//without locals, it gives us teh closure. with locals, the last local. so something is getting popped that shouldn't be.
		new VMTestCase("""
		               f = func(a,b,c,d){
		               456;
		               if(d == 0){
		                 return b
		               }else{
		               //push hi
		               //return top-of-stack
		                 return "hi"
		               }
		                 123;
		               };
		               
		               f(1,2,3,4)
		               """, new String("hi"));
		//new VMTestCase("a = func(b){if(b != 1){return b}else{1;2;3;return 0;4}};a(1)", new Integer(0));
	}

	[Test]
	public void TestExpressionBlockValuess()
	{
		new VMTestCase("a = {1;4;} a;", new Integer(4));
		//new VMTestCase("a = {}a;", VM.Null);
		//new VMTestCase("a = {0;b = if(true){1}else{2};b+2};a", new Integer(3));
		//new VMTestCase("a = {b=1;c=3;d=3;b+c+d};a+a", new Integer(14));

	}

	[Test]
	public void TestConditionals()
	{
		new VMTestCase("a = if (true) { 10;10 }else{0}; a", new Integer(10));
		new VMTestCase("a = if(false){3}else{1}; a+a", new Integer(2));
		new VMTestCase("if(true){3;3;3;3;}else{4;4;4;4;4;}", new Integer(3));
		new VMTestCase("if(false){3;3;3;3;}else{4;4;4;4;4;}", new Integer(4));
		//new VMTestCase("if(true){3}", new Integer(3));
		//new VMTestCase("if(true){if(false){0}else{5}}else{1}", new Integer(5));
		//new VMTestCase("if(4==2+2){1+2}else{0/0}", new Integer(3));
	}

	[Test]
	public void TestConditionalsWrappedInFunctions()
	{
		new VMTestCase("func f(){{1;2;3;1}};f()", new Integer(1));//1 on stack, function returns. 1 is returned. 
		new VMTestCase("if(true){3}else{4}", new Integer(3)); //leaves 3 on stack
		new VMTestCase("if(false){3}else{4}", new Integer(4)); //leaves 4 on stack
		new VMTestCase("if(true){3;3;3;3;}else{4;4;4;4;4;}", new Integer(3));
		new VMTestCase("func f(){if(true){3}else{4}};f()", new Integer(3));
		new VMTestCase("func f(){if(true){3;3;3;3;}else{4;4;4;4;4;}};f()", new Integer(3));
		new VMTestCase("func f(){1;return {if(false){3;3;3;3;}else{4}}};f()", new Integer(4));
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
		}else if(!eo.SameObjectData(ao)) //handle casts  )
		{
			Assert.Fail($" expected {eo.GetType()}. Expected {eo} is not a: {ao.ToString()}.");
			return false;
		}

		return true;
	}
}