using Object = scrub_lang.Objects.Object;

namespace scrub_lang.VirtualMachine;

//VM Report takes a virtual machine and provides usable information about it's state.
public class Status
{
	private VM _vm;
	
	public Status(VM vm)
	{
		_vm = vm;
	}

	public Dictionary<string, Object> Variables()
	{
		//this is tricky, because there's a few things going on. We have globals and locals. and builtins and so on.
		//also, the VM doesn't know the symbol table. We need it from the compiler, but the VM doesn't currently get a copy.
		return null;
	}
	
	public List<Object> Stack(int numItems)
	{
		//todo: cache list. Or at least don't reallocate. 
		List<Object> stack = new List<Object>();
		for (int i = 0; i < Math.Min(numItems, _vm.StackPointer); i++)
		{
			stack.Add(_vm.Stack[_vm.StackPointer-i]);
		}

		return stack;
	}
	
}