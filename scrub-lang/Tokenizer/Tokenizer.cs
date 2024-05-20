using System.Runtime.InteropServices.JavaScript;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

//The Tokenizer uses a simple class-based FSM. WHile it is overkill for many of the minimal lookaheads, iiiiiii like it.
//todo: bundle up "line,col" into a "location" struct.
/// <summary>
/// The Lexer
/// </summary>
///
public class Tokenizer
{
	private TokenizerStateBase? _state;
	private StreamReader _stream;
	private int CurrentLine = 0;
	private int CurrentColumn = 0;

	public List<Token> Tokens => _tokens;
	private List<Token> _tokens = new List<Token>();
	private bool isSyntaxError = false;//todo: status enum?
	private int _lastTokenProvided = -1;
	private int _prevCount;
	private string _returned = "";

	private ISourceProvider _source;

	public Tokenizer(string source)
	{
		_source = new StringProvider(source);
	}

	public Tokenizer(StreamReader source)
	{
		_source = new StreamProvider(source);
	}
	public Tokenizer(ISourceProvider source)
	{
		_source = source;
	}

	public Token Next()
	{
		//int count return...
		//Going to change this to a IEnumerator syntax. Foreach token in Tokenizer...
		//if we have a new token already.
		if (_lastTokenProvided < _tokens.Count - 1)
		{
			_lastTokenProvided++;
			_returned += _tokens[_lastTokenProvided].Literal;
			return _tokens[_lastTokenProvided];
		}
		//keep going until the size of tokens.count changes.
		while (_lastTokenProvided == _tokens.Count-1)
		{
			if (_source.TryGetNextCharacter(out var c, ref CurrentLine, ref CurrentLine))
			{
				ConsumeNext(c, CurrentLine, CurrentColumn);
			}
			else
			{
				//just keep returning break's so _states (e.g. identifier) exit.
				if (_state != null)
				{
					_state.Consume('\n', CurrentLine, CurrentColumn);
				}
				else
				{
					//just keep giving the parser EOF's. If it's doing lookahead, it can get as many EOF's as it wants.
					AddToken(new Token(TokenType.EOF,"",CurrentLine,CurrentColumn));
				}
			}
		}

		_lastTokenProvided++;
		_returned += _tokens[_lastTokenProvided].Literal;
		return _tokens[_lastTokenProvided];
	}

	public void ConsumeNext(char c, int l, int col)
	{
		//We get a c and just finished our previous token. We could be consuming anything.
		//Let's start with numbers. Numbers start with . or 0-9.
		//_state == null if this is called.
		if (_state != null)
		{
			_state.Consume(c,l,col);
			return;
		}
		
		//if _state==null, then we are on a new token.
		switch (c)
		{
			//math symbols that, on their own, are just tokens. (...unary)
			case ';':
				//we have to parse comments....
				AddToken(new Token(TokenType.EndExpression, c, l, col));
				return;
			case '+':
			case '-':
				_state = new IncrementTState(this);
				_state.Consume(c, l, col);
				return;
			case '*':
				AddToken(new Token(TokenType.Multiply, c, l, col));
				return;
			case '/':
				//we have to parse comments....
				_state = new CommentTState(this);
				_state.Consume(c,l,col);
				return;
			case '^':
				AddToken(new Token(TokenType.PowerOfXOR, c, l, col));
				return;
			case '%':
				AddToken(new Token(TokenType.Modulo, c, l, col));
				return;
			case '&':
			case '|':
				_state = new ComparisonTState(this);
				_state.Consume(c, l, col);
				return;
			case '~':
				AddToken(new Token(TokenType.BitwiseNot, c, l, col));
				return;
			case '"':
				_state = new StringTState(this);
				//we "consume" the first " by ignoring it.
				//_state.Consume(c,l,col);
				return;
			case ',':
				AddToken(new Token(TokenType.Comma, c, l, col));
				return;
			case '?':
				AddToken(new Token(TokenType.Question, c, l, col));
				return;
			case ':':
				AddToken(new Token(TokenType.Colon, c, l, col));
				return;
			case '{':
				AddToken(new Token(TokenType.StartExpressionBlock, c, l, col));
				return;
			case '}':
				AddToken(new Token(TokenType.EndExpressionBlock, c, l, col));
				return;
			case ' ':
			case '\t':
			case '\r':
			case '\n':
				//we don't have a concept as a whitespace token.
				//ignore whitespace unless we are inside one of our special cases, where it will end input.
				return;
			case '='://one or two or three equals (assignment, equals, and unexpected === aint a thing)
			case '>':
			case '<':
			case '!':
				_state = new EqualityTState(this);
				_state.Consume(c,l,col);//pass the buck
				return;
			case '(':
				AddToken(new Token(TokenType.OpenParen,c,l,col));
				return;
			case ')':
				AddToken(new Token(TokenType.CloseParen,c,l,col));
				return;
		}

		//handle numbers.
		if (char.IsDigit(c) || c == '.')
		{
			_state = new NumberTState(this);
			_state.Consume(c, l, col);
			return;
		}

		//Is it true that all literals must start with a letter?
		if (char.IsLetter(c))
		{
			_state = new IdentifierTState(this);
			_state.Consume(c,l,col);
			return;
		}
		
		//and finally, we consume thius remaining character: unexpected. WTF is this!
		AddToken(new Token(TokenType.Unexpected,c,l,col));
		return;
	}

	public void AddToken(Token token)
	{
		_tokens.Add(token);
		
		if (token.TokenType == TokenType.Unexpected)
		{
			var last = _tokens[^1];
			Console.WriteLine($"Uh oh, unexpected token \"{last.Literal}\" on line {last.Line}, column {last.Column}.");
			isSyntaxError = true;//kills our consumption.
		}
	}
	
	public void SwitchState(TokenizerStateBase? newStateBase)
	{
		_state = newStateBase;
	}

	public void ExitState(TokenizerStateBase oldState)
	{
		if (oldState == _state)
		{
			_state = null;
		}
		else
		{
			Console.WriteLine("SHit!");
		}

		_state = null;
	}
}

