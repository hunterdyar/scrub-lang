﻿using scrub_lang.VirtualMachine;

namespace scrub_lang.Objects;

//not sure I love having these in Objects.
public static class Builtins
{
	//just realized while the compiler wants us to use an array for builtins.... its for an operand.
	public static (string Name, Builtin Builtin)[] AllBuiltins => _builtins;
	private static(string Name, Builtin Builtin)[] _builtins;

	static Builtins()
	{
		_builtins = new (string, Builtin)[3];
		_builtins[0] = ("print", new Builtin(Print));
		_builtins[1] = ("len",new Builtin(Len));
		_builtins[2] = ("push",new Builtin(Push));
		//todo: first
		//todo: last
		//todo: abs
		//todo: sin
		//todo: cos, etc.
	}

	public static Builtin? GetBuiltinByname(string name)
	{
		foreach (var b in _builtins)
		{
			if (b.Name == name)
			{
				return b.Builtin;
			}
		}

		return null;
	}

	static Object? Len(Object[] args)
	{
		if (args.Length != 1)
		{
			NewError($"Wrong number of arguments for Len. Need 1, got {args.Length}.");
		}

		switch (args[0].GetType())
		{
			case ScrubType.Array:
				return ((Array)args[0]).Length;
			case ScrubType.String:
				return new Integer(args[0].Bytes.Length);
			default:
				return NewError($"Cannot get Len of {args[0].GetType()}.");
				//or: yes, we can get the number of bytes of ANY data type! but that doesn't make sense for ints, it would always be 4
				return new Integer(args[0].Bytes.Length);
		}
	}

	static Object? Print(Object[] args)
	{
		foreach (var arg in args)
		{
			//todo: get the appropriate textwriter.
			Console.WriteLine(arg);
		}

		//return new Null();//this might break things, because we are comparing by reference to the VM null.
		return null;
	}

	static Object? Push(Object[] args)
	{
		if (args.Length != 2)
		{
			return NewError($"wrong number of args for push. Got {args.Length}, need 2");
		}

		if (args[0].GetType() == ScrubType.Array)
		{
			var arr = (Array)args[0];
			var l = arr.Length.NativeInt;
			var newElements = new Object[l + 1];
			System.Array.Copy(arr.NativeArray,newElements,l);
			newElements[l] = args[1];
			return new Array(newElements);
		}else if (args[0].GetType() == ScrubType.String)
		{
			var str = (String)args[0];
			var nstr = str.ToNativeString();
			var l = nstr.Length;
			nstr = nstr+args[1].ToString();
			return new String(nstr);
		}
		else
		{
			return NewError($"cannot push onto type {args[0].GetType()}");
		}
	}

	static public Object NewError(string message)
	{
		//todo: update this when we make an error type...
		throw new VMException(message);
	}
}