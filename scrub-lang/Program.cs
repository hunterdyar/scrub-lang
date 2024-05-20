
using System.Diagnostics;
using System.Text;
using scrub_lang.Parser;
using scrub_lang.Tokenizer;

static class Scrub
{
	public static async Task Main()
	{
		// try
		// {
			// Create an instance of StreamReader to read from a file.
			// The using statement also closes the StreamReader.
			Console.WriteLine("Starting Scrub");
			
			using (StreamReader stream = new StreamReader("test/test.scrub"))
			{
				Tokenizer t = new Tokenizer(stream);
				Parser p = new Parser(t);
				var result = p.ParseExpression();
				var sb = new StringBuilder();
				result.Print(sb);
				Console.WriteLine(sb.ToString());
			}
		// }
		// catch (Exception e)
		// {
		// 	// Let the user know what went wrong.
		// 	Console.WriteLine("uh oh:");
		// 	Console.WriteLine(e.Message);
		// }
	}
}