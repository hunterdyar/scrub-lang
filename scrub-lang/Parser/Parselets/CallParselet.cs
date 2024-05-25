using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class CallParselet : IInfixParselet
{
	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		// Parse the comma-separated arguments until we hit, ')'.
		List<IExpression> args = new List<IExpression>();

		// There may be no arguments at all.
		if (!parser.Match(TokenType.CloseParen))
		{
			do
			{
				args.Add(parser.ParseExpression());
			} while (parser.Match(TokenType.Comma));

			parser.Consume(TokenType.CloseParen);
		}

		return new CallExpression(left, args, left.Location);//location is the identififer, not the (
	}

	public int GetBindingPower()
	{
		return BindingPower.Call;
	}
}