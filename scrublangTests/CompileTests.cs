using System.Diagnostics;
using System.Text;
using NuGet.Frameworks;
using NUnit.Framework.Internal;
using scrub_lang.Compiler;
using scrub_lang.Objects;
using scrub_lang.Parser;
using scrub_lang.VirtualMachine;

namespace scrub_lang;

public class CompileTests
{

	[Test]
	public void TestExpressionBlockPopCorrectly()
	{
			CompileTest("{1}",
			[new Integer(1)],
			Op.Make(OpCode.OpConstant, 0),
			Op.Make(OpCode.OpPop));
			CompileTest("{1;1}",
				[new Integer(1)],
				Op.Make(OpCode.OpConstant, 0),
				Op.Make(OpCode.OpPop),
				Op.Make(OpCode.OpConstant, 0),
				Op.Make(OpCode.OpPop)
				);
			CompileTest("{1;1;1}",
				[new Integer(1)],
				Op.Make(OpCode.OpConstant, 0),
				Op.Make(OpCode.OpPop),
				Op.Make(OpCode.OpConstant, 0),
				Op.Make(OpCode.OpPop),
				Op.Make(OpCode.OpConstant, 0),
				Op.Make(OpCode.OpPop)
				);
				CompileTest("a = {1}",
					[new Integer(1)],
					Op.Make(OpCode.OpConstant, 0),
					Op.Make(OpCode.OpSetGlobal, 0),
					Op.Make(OpCode.OpPop)
					);
	}
	[Test]
	public void TestConditionalCompile()
	{
		CompileTest("if(false){3}else{1}",
			[new Integer(3),new Integer(1)],
			Op.Make(OpCode.OpFalse),
			Op.Make(OpCode.OpJumpNotTruthy,4,0),
			Op.Make(OpCode.OpConstant,0),
			Op.Make(OpCode.OpJump,5,0),
			Op.Make(OpCode.OpConstant, 1),
			Op.Make(OpCode.OpPop));
		
		
		//we don't want pops until after both branches. i think when we skip the alternative, we're skipping or not skipping a pop.
		//but sometimes that compiles the same (pop at end of alt or pop at end of if)
		// CompileTest("if (true) { 10;10 }else{0}",
		// 	[new Integer(10), new Integer(0)],
		// 	Op.Make(OpCode.OpTrue),
		// 	Op.Make(OpCode.OpJumpNotTruthy, 7),//it's the index+1 we want?
		//
		// 	Op.Make(OpCode.OpConstant, 0), //consequence
		// 	Op.Make(OpCode.OpPop),
		// 	Op.Make(OpCode.OpConstant, 0), //consequence
		//
		// 	Op.Make(OpCode.OpJump, 8),
		// 	Op.Make(OpCode.OpConstant,1),//alterantive
		// 	
		// 	Op.Make(OpCode.OpPop)
		// );
	} 
	
	[Test]
	public void TestCompile()
	{
		//0 and 1 are the locations in the constants pool.
		CompileTest("1 + 2", [new Integer(1),new Integer(2)], Op.Make(OpCode.OpConstant, 0), Op.Make(OpCode.OpConstant, 1), Op.Make(OpCode.OpAdd), Op.Make(OpCode.OpPop));		CompileTest("1 + 2", [new Integer(1),new Integer(2)], Op.Make(OpCode.OpConstant, 0), Op.Make(OpCode.OpConstant, 1), Op.Make(OpCode.OpAdd), Op.Make(OpCode.OpPop));
		// CompileTest("if (true) { 10 } \n 3333\n",
		// 	[new Integer(10), new Integer(20), new Integer(3333)],
		// 	Op.Make(OpCode.OpTrue),
		// 	Op.Make(OpCode.OpJumpNotTruthy, 4,1),
		// 	Op.Make(OpCode.OpConstant, 0),
		// 	Op.Make(OpCode.OpJump, 5,1),
		// 	Op.Make(OpCode.OpNull),
		// 	Op.Make(OpCode.OpPop),
		// 	Op.Make(OpCode.OpConstant, 1),
		// 	Op.Make(OpCode.OpPop)
		// 	);
		
		CompileTest("one = 1\n two = 2\n",
			[new Integer(1), new Integer(2)],
			Op.Make(OpCode.OpConstant, 0),
			Op.Make(OpCode.OpSetGlobal, 0),
			Op.Make(OpCode.OpPop),
			Op.Make(OpCode.OpConstant,1),
			Op.Make(OpCode.OpSetGlobal,1),
			Op.Make(OpCode.OpPop)
		);
		CompileTest("one = 1\n one",
			[new Integer(1)],
			Op.Make(OpCode.OpConstant, 0),
			Op.Make(OpCode.OpSetGlobal, 0),
			Op.Make(OpCode.OpPop),
			Op.Make(OpCode.OpGetGlobal, 0),
			Op.Make(OpCode.OpPop)
		);
		CompileTest("one = 1\n two = one\ntwo",
			[new Integer(1)],
			Op.Make(OpCode.OpConstant, 0),
			Op.Make(OpCode.OpSetGlobal, 0),
			Op.Make(OpCode.OpPop),
			Op.Make(OpCode.OpGetGlobal, 0),
			Op.Make(OpCode.OpSetGlobal, 1),
			Op.Make(OpCode.OpPop),
			Op.Make(OpCode.OpGetGlobal, 1),
			Op.Make(OpCode.OpPop)
		);
	}

	public static void CompileTest(string input, object[] expectedConstants, params int[] expectedInstructions)
	{
		try
		{
			var t = new Tokenizer.Tokenizer(input);
			var p = new Parser.Parser(t);
			var ast = p.ParseProgram();
			var c = new Compiler.Compiler();
			var e = c.Compile(ast);
			if (e != null)
			{
				Assert.Fail(e.Message);
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

			if (!expectedInstructions.SequenceEqual(byteCode.Instructions))
			{
				StringBuilder sb = new StringBuilder();
				if (!failed)
				{
					sb.AppendLine("[FAIL] Source: " + input);
					failed = true;
				}
				sb.AppendLine("       Expected Instructions:\n " + Op.InstructionsToString(expectedInstructions));
				sb.AppendLine("       Actual Instructions:\n " + Op.InstructionsToString(byteCode.Instructions));
				Assert.Fail(sb.ToString());

			}
		}catch (CompileException cx)
		{
			Assert.Fail(cx.Message);
		}
	}
}