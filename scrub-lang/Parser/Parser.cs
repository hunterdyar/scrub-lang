using scrub_lang.Parser.Parselets;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser;

public class Parser
{
	private Tokenizer.Tokenizer _tokenizer;
	private List<Token> _readTokens = new List<Token>();
	private Dictionary<TokenType, IPrefixParselet> _prefixParselets = new Dictionary<TokenType, IPrefixParselet>();
	private Dictionary<TokenType, IInfixParselet> _infixParselets = new Dictionary<TokenType, IInfixParselet>();

	public Parser(Tokenizer.Tokenizer tokenizer)
	{
		
		_tokenizer = tokenizer;
		//in prefix dictionary
		Register(TokenType.Identifier,new IdentifierParselet());
		
		Register(TokenType.Assignment, new AssignParselet());
		Register(TokenType.Question, new TernaryParselet());
		Register(TokenType.OpenParen, new InlineGroupParselet());
		Register(TokenType.OpenParen, new CallParselet());
		Register(TokenType.VarKeyword, new VariableDeclarationParselet());
		Register(TokenType.FunctionKeyword, new FunctionDeclarationParselet());
		Register(TokenType.StartExpressionBlock, new ExpressionGroupParselet());
		Register(TokenType.NumberLiteral, new LiteralParselet());
		Register(TokenType.String, new LiteralParselet());
		
		//+1, -1, ~1, !true
		Prefix(TokenType.Plus, BindingPower.Prefix);
		Prefix(TokenType.Minus, BindingPower.Prefix);
		Prefix(TokenType.BitwiseNot, BindingPower.Prefix);
		Prefix(TokenType.Bang, BindingPower.Prefix);
		
		//postix:
		//++
		
		InfixLeft(TokenType.Plus,BindingPower.Sum);
		InfixLeft(TokenType.Minus,BindingPower.Sum);
		InfixLeft(TokenType.Multiply, BindingPower.Product);
		InfixLeft(TokenType.Division, BindingPower.Product);
		InfixRight(TokenType.PowerOfXOR, BindingPower.Exponent);
	}

	#region RegisterUtility

	public void Register(TokenType token, IPrefixParselet parselet)
	{
		_prefixParselets.Add(token, parselet);
	}

	public void Register(TokenType token, IInfixParselet parselet)
	{
		_infixParselets.Add(token, parselet);
	}

	/// <summary>
	/// Registers a postfix unary operator parselet for the given token and binding power.
	/// </summary>
	public void Postfix(TokenType token, int bindingPower)
	{
		Register(token, new PostfixOperatorParselet(bindingPower));
	}

	/// <summary>
	/// Registers a prefix unary operator parselet for the given token and binding power.
	/// </summary>
	public void Prefix(TokenType token, int bindingPower)
	{
		Register(token, new PrefixOperatorParselet(bindingPower));
	}

	/// <summary>
	///  Registers a left-associative binary operator parselet for the given token and binding power.
	/// </summary>
	public void InfixLeft(TokenType token, int bindingPower)
	{
		Register(token, new BinaryOperatorParselet(bindingPower, false));
	}

	/// <summary>
	/// Registers a right-associative binary operator parselet for the given token and binding power.
	/// </summary>
	public void InfixRight(TokenType token, int bindingPower)
	{
		Register(token, new BinaryOperatorParselet(bindingPower, true));
	}

	#endregion
	public IExpression ParseExpression(int precedence = 0)
	{
		var token = Consume();
		
		//Skip over ;'s
		if (token.TokenType == TokenType.EndExpression)
		{
			token = Consume(TokenType.EndExpression);
		}
		
		if (!_prefixParselets.TryGetValue(token.TokenType, out var prefix))
		{
			throw new ParseException($"Could not parse (prefix) \"{token.Literal}\" ({token.TokenType}) at {token.Line},{token.Column}");
		}

		IExpression left = prefix.Parse(this, token);

		while (precedence < GetBindingPower())
		{
			token = Consume();

			if (!_infixParselets.TryGetValue(token.TokenType, out var infix))
			{
				throw new ParseException("Could not parse (infix) \"" + token.Literal + $"\"");
			}

			left = infix.Parse(this, left, token);
		}

		return left;
	}

	public bool Match(TokenType expected)
	{
		var token = LookAhead(0);
		if (token.TokenType != expected)
		{
			return false;
		}

		Consume();
		return true;
	}
	public Token Consume(TokenType expected)
	{
		var token = LookAhead(0);
		if (token.TokenType != expected)
		{
			throw new ParseException("Expected token " + expected + " and found " + token.TokenType);
		}

		return Consume();
	}
	public Token Consume()
	{
		var token = LookAhead(0);
		_readTokens.Remove(token);
		return token;
	}

	public bool Peek(TokenType expected)
	{
		return LookAhead(0).TokenType == expected;
	}
	private Token LookAhead(int distance)
	{
		while (distance >= _readTokens.Count)
		{
			_readTokens.Add(_tokenizer.Next());
		}
		// Get the queued token.
		return _readTokens[distance];
	}

	private int GetBindingPower()
	{
		if (_infixParselets.TryGetValue(LookAhead(0).TokenType, out var parselet))
		{
			return parselet.GetBindingPower();
		}

		return 0;
	}
}