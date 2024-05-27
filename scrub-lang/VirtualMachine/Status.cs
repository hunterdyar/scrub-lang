using scrub_lang.Compiler;
using scrub_lang.Objects;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.VirtualMachine;

//VM Report takes a virtual machine and provides usable information about it's state.
public class Status
{
	private VM _vm;
	private SymbolTable Symbols => _vm.Symbols;
	public Status(VM vm)
	{
		_vm = vm;
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

	public List<VariableState> GetVariables()
	{
		Dictionary<string, VariableState> state = new Dictionary<string, VariableState>();
		//from bottom to top.
		foreach (var frame in  _vm.Frames)
		{
			foreach (var symbol in frame.closure.CompiledFunction.Symbols.Table.Values)
			{
				if (symbol.Scope == ScopeDef.Builtin || symbol.Scope == ScopeDef.Function)
				{
					continue;
				}
				var vs = new VariableState();
				vs.Name = symbol.Name;
				vs.Scope = symbol.Scope;
				if (symbol.Scope == ScopeDef.Global)
				{
					vs.Object = _vm.Globals[symbol.Index];
				}else if (symbol.Scope == ScopeDef.Local)
				{
					vs.Object = _vm.Stack[frame.basePointer + symbol.Index];
				}
				state.Add(vs.Name,vs);//get error if we already have it.
			}
		}

		//ehhh
		return state.Values.ToList();
	}
}

public class VariableState()
{
	public string Name;
	public ScopeDef Scope;
	public int HeapLocation = -1;
	public Object Object;
	public ScrubType Type => Object.GetType();
}