using scrub_lang.Compiler;
using scrub_lang.Parser;
using scrub_lang.VirtualMachine;
using Environment = scrub_lang.Evaluator.Environment;

static class Scrub
{
	

	public static async Task<int> Main()
	{
		//Repl
		await Repl(Console.In, Console.Out);
		return 0;
	}

	public static async Task Repl(TextReader reader, TextWriter writer)
	{
		bool repel = true;
		Environment env = new Environment();
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

			var output = await Execute(line, env);
			writer.WriteLineAsync(output);
		}
	}

	public static async Task<string> Execute(string input, Environment? environment)
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

		var comp = new Compiler();
		
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

		var vm = new VM(comp.ByteCode());
		try
		{
			var vmerror = vm.Run();
			if (vmerror != null)
			{
				return vmerror.ToString();
			}
		}
		catch (VMException vme)
		{
			return vme.Message;
		}

		return vm.LastPopped()?.ToString();
	}
}