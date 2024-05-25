using System.Diagnostics;
using System.Security.Cryptography;
using scrub_lang;
using scrub_lang.Compiler;
using scrub_lang.Evaluator;
using scrub_lang.Parser;
using scrub_lang.VirtualMachine;
using Environment = scrub_lang.Evaluator.Environment;
using Object = scrub_lang.Objects.Object;

static class Scrub
{
	public static async Task<int> Main(string[] args)
	{
		if (args.Length == 0)
		{
			//Repl
			await Repl(Console.In, Console.Out);
			return 0;
		}
		else
		{
			Stopwatch sw = new Stopwatch();
			string program = await File.ReadAllTextAsync(args[0]);
			sw.Start();
			var result = Execute(program);
			Console.WriteLine(result);
			sw.Stop();
			var ts = new TimeSpan(sw.ElapsedTicks);
			Console.WriteLine($"execution completed in {ts.TotalMilliseconds}ms");
		}

		return 0;
	}

	public static async Task Repl(TextReader reader, TextWriter writer)
	{
		bool repel = true;
		//compiler env
		Environment env = new Environment();
		//VM env
		Object[]? globals = null;
		while (repel)
		{
			Console.Write("~~> ");
			string? line = await reader.ReadLineAsync();
			if (line == null)
			{
				break;
			}
			else if (line == "exit" || line == "quit")
			{
				break;
			}
			
			var output = Execute(line, ref env, ref globals);
			writer.WriteLineAsync(output);
		}
	}
	public static string Execute(string input)
	{
		var env = new Environment();
		Object[]? globals = null;
		return Execute(input, ref env, ref globals);
	}
	public static string Execute(string input, ref Environment? environment, ref Object[]? globals)
	{
		if (environment == null)
		{
			environment = new Environment();
		}

		IExpression p;
		try
		{
			p = VM.Parse(input);
		}
		catch (ParseException pe)
		{
			return pe.Message;
		}

		var comp = environment == null ? new Compiler() : new Compiler(environment);
		
		try
		{
			var error = comp.Compile(p);
			if (error != null)
			{
				//testException
				return error.ToString();
			}
		}
		catch (CompileException ce)
		{
			return ce.Message;
		}
		
		var vm = globals == null ? new VM(comp.ByteCode()) :  new VM(comp.ByteCode(), globals);
		//Console.WriteLine("Executing Bytecode instructions:");
		//var s = Op.InstructionsToString(vm.ByteCode.Instructions);
		//Console.WriteLine(s);
		//Console.WriteLine("---");
		try
		{
			ScrubVMError? vmerror = null;
			vmerror = vm.RunOne();//start. run until paused.
			vm.Pause();
			while (vm.State == VMState.Paused || vm.State == VMState.Complete)
			{
				
				Console.WriteLine("---paused, any key to resume---");
				var x = Console.ReadKey();
				if (x.Key == ConsoleKey.D || x.Key == ConsoleKey.RightArrow)
				{
					vmerror = vm.RunOne();//todo 'next and previous' to handle state changing and interface for 'runone, previous one'.
					if (vmerror != null)
					{
						return vmerror.ToString();
					}
					else
					{
						Console.WriteLine(vm.LastPopped()?.ToString());
					}
				}else if (x.Key == ConsoleKey.A || x.Key == ConsoleKey.LeftArrow)
				{
					vmerror = vm.PreviousOne();
					if (vmerror != null)
					{
						return vmerror.ToString();
					}
				}
				else
				{
					if (vm.State == VMState.Paused)
					{
						vmerror = vm.Run();
					}
					else
					{
						break;
					}
				}
			}

			if (vmerror != null)
			{
				return vmerror.ToString();
			}
		}
		catch (VMException vme)
		{
			return vme.Message;
		}

		environment = comp.Environment();
		globals = vm.Globals;
		return vm.LastPopped()?.ToString();
	}
}