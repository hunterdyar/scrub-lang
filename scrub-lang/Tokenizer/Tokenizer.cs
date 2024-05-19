using System.Runtime.InteropServices.JavaScript;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;

//The Tokenizer uses a simple class-based FSM. WHile it is overkill for many of the minimal lookaheads, iiiiiii like it.
//todo: bundle up "line,col" into a "location" struct.
public class Tokenizer
{
	private TokenizerStateBase? _state;
	private StreamReader _stream;
	private int CurrentLine;
	private int CurrentColumn;

	public List<Token> Tokens => _tokens;
	private List<Token> _tokens = new List<Token>();
	private bool isSyntaxError = false;//todo: status enum?
	
	public async Task TokenizeStream(string path)
	{
		using (StreamReader stream = new StreamReader(path))
		{
			_tokens = new List<Token>();
			CurrentLine = 0;
			_stream = stream;
			isSyntaxError = false;
			bool streaming = true;
			while (streaming)
			{
				string? line = await _stream.ReadLineAsync();
				CurrentLine++;
				if (line == null)
				{
					//Force end-of-file. whitespace will ensure any remaining tokens/literals end.
					ConsumeNext('\n',CurrentLine,0);
					streaming = false;
					AddToken(new Token(TokenType.EOF, "", CurrentLine, 0));
					break;
				}

				ConsumeLine(line);
				if (isSyntaxError)
				{
					streaming = false;
					break;
				}
			}
		}
	}

	public async Task TokenizeString(string source)
	{
		_tokens = new List<Token>();
		CurrentLine = 0;
		isSyntaxError = false;
		var splits = source.Split('\n');
		foreach(var line in splits)
		{
			CurrentLine++;
			ConsumeLine(line);
			if (isSyntaxError)
			{
				break;
			}
		}

		ConsumeNext('\n', CurrentLine, 0);
		AddToken(new Token(TokenType.EOF, "", CurrentLine, 0));
	}

	public void ConsumeLine(string line, bool consumeAdditionalLineEnd = true)
	{
		Console.WriteLine($"Consume Line: {line}");
		for (CurrentColumn = 0; CurrentColumn < line.Length; CurrentColumn++)
		{
			char c = line[CurrentColumn];

			ConsumeNext(c, CurrentLine, CurrentColumn);
			if (isSyntaxError)
			{
				break;
			}
		}

		if (consumeAdditionalLineEnd)
		{
			ConsumeNext('\n',CurrentLine,CurrentColumn+1);
		}
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
				AddToken(new Token(TokenType.Plus,c,l,col));
				return;
			case '-':
				AddToken(new Token(TokenType.Minus,c,l,col));
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
				AddToken(new Token(TokenType.PowerOf, c, l, col));
				return;
			case '%':
				AddToken(new Token(TokenType.Modulo, c, l, col));
				return;
			case '"':
				_state = new StringTState(this);
				//we "consume" the first " by ignoring it.
				//_state.Consume(c,l,col);
				return;
			case ',':
				AddToken(new Token(TokenType.Comma, c, l, col));
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

