using System.Net.Mime;
using System.Text;
using scrub_lang.Compiler;
using scrub_lang.Objects;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.VirtualMachine;

//VM Report takes a virtual machine and provides usable information about it's state.
public class Status
{
	private List<Object> _stack;
	private VM _vm;
	private SymbolTable Symbols => _vm.Symbols;
	
	public Status(VM vm)
	{
		_vm = vm;
	}
	
	
	public List<Object> Stack(int numItems)
	{
		//todo: we can avoid the add/removes if we know how many recents we want, and keep the stack that large.
		_stack.Clear();
		for (int i = 0; i < Math.Min(numItems, _vm.StackPointer); i++)
		{
			_stack.Add(_vm.Stack[_vm.StackPointer-i]);
		}

		return _stack;
	}

	public List<VariableState> GetVariables()
	{
		Dictionary<string, VariableState> state = new Dictionary<string, VariableState>();
		//from bottom to top.
		foreach (var frame in  _vm.Frames.Reverse())
		{
			if (frame.closure?.CompiledFunction?.Symbols == null)
			{
				continue;
			}
			foreach (var symbol in frame.closure.CompiledFunction.Symbols.Table.Values)
			{
				if (state.ContainsKey(symbol.Name))
				{
					//Note that we are looping from end to beginning. So any key that already exists will the most local, shadowing the others.
					//so we don't add them. In the future, we will want to display this so feedback on variable name hiding.
					continue;
				}
				
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