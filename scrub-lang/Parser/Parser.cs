﻿using scrub_lang.Parser.Parselets;
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
		
		//in prefix dictionary. These are how we start any expression.
		Register(TokenType.Identifier,new IdentifierParselet());
		Register(TokenType.TrueKeyword, new IdentifierParselet());
		Register(TokenType.FalseKeyword, new IdentifierParselet());
		Register(TokenType.NullKeyword, new IdentifierParselet());
		Register(TokenType.Assignment, new AssignParselet());
		Register(TokenType.Question, new TernaryParselet());
		Register(TokenType.OpenParen, new InlineGroupParselet());//prefix
		Register(TokenType.OpenParen, new CallParselet());//infix
		Register(TokenType.FunctionKeyword, new FunctionDeclarationParselet());
		Register(TokenType.StartExpressionBlock, new ExpressionGroupParselet());
		Register(TokenType.NumberLiteral, new LiteralParselet());
		Register(TokenType.HexLiteral, new LiteralParselet());
		Register(TokenType.OctalLiteral, new LiteralParselet());
		Register(TokenType.BinaryLiteral, new LiteralParselet());
		Register(TokenType.IfKeyword,new IfParselet());
		Register(TokenType.String, new LiteralParselet());
		Register(TokenType.ReturnKeyword,new ReturnParselet());
		Register(TokenType.ImportKeyword, new ImportParselet());
		Register(TokenType.OpenBracket,new ArrayLiteralParselet());//prefix
		Register(TokenType.OpenBracket,new ArrayLookupParselet());//infix
		Register(TokenType.Break, new BreakInfixParselet());//break just returns left, but has a low binding power.
		
		//Register(TokenType.Break, new BreakPrefixParselet());//as a prefix, it just returns parse().
		//+1, -1, ~1, !true
		Prefix(TokenType.Plus, BindingPower.UnarySum);
		Prefix(TokenType.Minus, BindingPower.UnarySum);
		Prefix(TokenType.BitwiseNot, BindingPower.BitwiseNot);
		Prefix(TokenType.Bang, BindingPower.Not);
		
		//postfix's
		Postfix(TokenType.IncrementConcatenate, BindingPower.Increment);
		Postfix(TokenType.Decrement, BindingPower.Increment);
		
		//infix operators... most of them (a op b)
		
		//math
		InfixLeft(TokenType.Plus,BindingPower.Sum);
		InfixLeft(TokenType.Minus,BindingPower.Sum); 
		InfixLeft(TokenType.Modulo,BindingPower.Modulo);
		InfixLeft(TokenType.Multiply, BindingPower.Product);
		InfixLeft(TokenType.Division, BindingPower.Product);
		
		//conditional
		InfixLeft(TokenType.NotEquals, BindingPower.Equality);
		InfixLeft(TokenType.EqualTo, BindingPower.Equality);
		InfixLeft(TokenType.GreaterThan, BindingPower.NumericCompare);
		InfixLeft(TokenType.LessThan, BindingPower.NumericCompare);
		InfixLeft(TokenType.GreaterThanOrEqualTo, BindingPower.NumericCompare);
		InfixLeft(TokenType.LessThanOrEqualTo, BindingPower.NumericCompare);
		InfixLeft(TokenType.And, BindingPower.LogicalAnd);
		InfixLeft(TokenType.Or, BindingPower.LogicalOr);

		//bitwise
		InfixLeft(TokenType.BitwiseLeftShift, BindingPower.BitwiseShift);
		InfixLeft(TokenType.BitwiseRightShift, BindingPower.BitwiseShift);
		InfixLeft(TokenType.BitwiseAnd, BindingPower.BitwiseAnd);
		InfixLeft(TokenType.BitwiseOr, BindingPower.BitwiseOr);
		InfixLeft(TokenType.BitwiseXOR, BindingPower.BitwiseXor);
		
		//todo concat parsing.
		//string
		//infix(IncrementConcatenate). gotta disambiguate between left++ and left++right. that's done with binding power, usually, right? so ... how do i differentiate... im skipping for now.
		
		InfixRight(TokenType.PowerOf, BindingPower.Exponent);
	}

	public IExpression ParseProgram()
	{
		List<IExpression> expressions = new List<IExpression>();
		bool compete = false;
		//Parse until we end up with null which we get at EOF or some surviveable error.
		do
		{
			var e = ParseExpression();
			if (e != null)
			{
				expressions.Add(e);
			}
			else
			{
				compete = true;
			}
		} while (!compete);

		if (expressions.Count == 0)
		{
			//Create a new expression that pushes and pops a null.
			//todo: it should just be some kind of Null. this is really an edge-case avoidance.
			var exp = new List<IExpression>();
			//exp.Add(new NullExpression(new Location(0, 0)));
			return new ProgramExpression(exp, new Location(0, 0));
			return new NullExpression(new Location(0,0));
		}
		if (expressions.Count == 1)
		{
			return new ProgramExpression(expressions, expressions[0].Location);
		}
		else
		{
			//todo: empty?
			return new ProgramExpression(expressions, expressions[0].Location);//i guess
		}
	}
	
	public IExpression ParseExpression(int precedence = 0)
	{
		var token = Consume();
		
		//Skip over ;'s
		//We can't actually do this, will need to handle Break as a unary that has very low precedence.
		//Or maybe as a postFix that just returns the left side of the expression?
		//this is an edge-case, won't handle multiple ;;;;'s
		while (token.TokenType == TokenType.Break)
		{
			token = Consume();
		}
		
		if (token.TokenType == TokenType.EOF)
		{
			return null;
		}

		//parse extra or beginning ;'s
		 
		
		if (!_prefixParselets.TryGetValue(token.TokenType, out var prefix))
		{
			throw new ParseException($"Could not parse (prefix) \"{token.Literal}\" ({token.TokenType}) at {token.Location}");
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
}