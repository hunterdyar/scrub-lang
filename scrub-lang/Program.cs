
using System.Diagnostics;
using scrub_lang.Tokenizer;

static class Scrub
{
	public static async Task Main()
	{
		try
		{
			// Create an instance of StreamReader to read from a file.
			// The using statement also closes the StreamReader.
			Console.WriteLine("Starting Scrub");
		
			Tokenizer t = new Tokenizer();
			await t.TokenizeStream("test/test.scrub");

			Console.WriteLine($"Done. {t.Tokens.Count} Tokens:");

			foreach (var token in t.Tokens)
			{
				Console.Write(token.TokenType.ToString()+", ");
			}
			
		}
		catch (Exception e)
		{
			// Let the user know what went wrong.
			Console.WriteLine("The file could not be read:");
			Console.WriteLine(e.Message);
		}
	}
}