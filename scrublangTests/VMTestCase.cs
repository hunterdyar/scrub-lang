using System.Runtime.Intrinsics.X86;
using scrub_lang.Parser;

namespace scrub_lang.VirtualMachine;

	public enum TestType
	{
		RunToEnd,
		ZigZagWithUndo
	}
	public struct VMTestCase
	{
		public string input;
		public object expected;
		private IExpression parsedInput;
		private Compiler.Compiler comp;
		private VirtualMachine.VM vm;
		private Object[] _stackCache = new Object[VM.StackSize];
		public VMTestCase(string input,object expected, TestType ttype = TestType.RunToEnd)
		{
			this.expected = expected;
			this.input = input;
			
			parsedInput = Scrub.Parse(input);
			comp = new Compiler.Compiler();

			var error = comp.Compile(parsedInput);
			if (error != null)
			{
				throw new VMException(error.ToString());
			}

			vm = new VM(comp.GetProgram());
			if (ttype == TestType.RunToEnd)
			{
				TestCase();
			}
			else if(ttype == TestType.ZigZagWithUndo)
			{
				TestWithUndosZigZag();
			}
		}

		public bool TestCase()
		{
			var vmerror = vm.Run();
			if (vmerror != null)
			{
				throw new VMException(vmerror.ToString());
			}

			var top = vm.LastPopped();
			var b= VMTests.CompareObjects(expected, top);
			Assert.IsTrue(b);
			return b;
		}
		//two steps forward, one step back.
		public bool TestWithUndosZigZag()
		{
			vm.Pause();
			while (vm.State == VMState.Initialized || vm.State == VMState.Paused)
			{
				var t = TestStepAndUndo();
				Assert.IsTrue(t);
				var e = vm.RunOne();
				if (e != null)
				{
					break;
				}
			}

			var top = vm.LastPopped();
			var b = VMTests.CompareObjects(expected, top);
			Assert.IsTrue(b);
			return b;
		}

		public bool TestStepAndUndo()
		{
			if (vm.State == VMState.Complete)
			{
				return true;
			}
			//copy state
			Array.Copy(vm.Stack,_stackCache,vm.StackPointer);
			int ip = vm.InstructionPointer;
			vm.RunOne();
			var log = vm.Log.LatestOperation;
			// if (vm.State == VMState.Complete)
			// {
			// 	//todo: going from complete 
			// 	return true;
			// }
			vm.PreviousOne();
			//This isn't true..... if we undo a function call, we enter it's frame (at the back); and ip is current frame's pointer.
			//calling a function is a "forward" action, it loads up the function to be undone...
			//going back one step in the current frame would be undoing until we return to this frame.
			//if we don't do this, we get an infinite loop, because we keep testing the end uselessly, i think?
			Assert.AreEqual(ip,vm.InstructionPointer,"Instruction Pointers Not Equal. Last Log:" + log);
			for (int i = 0; i < vm.StackPointer; i++)
			{
				Assert.IsTrue(VMTests.CompareObjects(vm.Stack[i], _stackCache[i]),"Stack not equal. Last Log:"+log);
			}
			//copy state, compare with copy.
			
			return true;
		}
	}
