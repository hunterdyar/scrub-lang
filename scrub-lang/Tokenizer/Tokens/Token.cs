using System.Runtime.CompilerServices;

namespace scrub_lang.Tokenizer.Tokens;

public class Token
{
	public readonly TokenType TokenType;
	public readonly string Literal;
	public readonly int Line;
	public readonly int Column;

	public Token(TokenType tokenType, string literal, int line, int column)
	{
		this.TokenType = tokenType;
		this.Literal = literal;
		this.Line = line;
		this.Column = column;
	}
	public Token(TokenType tokenType, char literal, int line, int column)
	{
		this.TokenType = tokenType;
		this.Literal = literal.ToString();
		this.Line = line;
		this.Column = column;
	}

	public string ToString()
	{
		return Literal;
	}

	public static string OperatorToString(TokenType tokenType)
	{
		switch(tokenType)
		{
			case Tokens.TokenType.Assignment:
				return "=";
			case TokenType.Plus:
				return "+";
			case TokenType.Bang:
				return "!";
			case TokenType.Comma:
				return ",";
			case TokenType.Minus:
				return "-";
			case TokenType.BitwiseXOR:
				return "^";
			case TokenType.PowerOf:
				return "**";
			case TokenType.Question:
				return "?";
			case TokenType.Colon:
				return ":";
			case TokenType.BitwiseAnd:
				return "&";
			case TokenType.Multiply:
				return "*";
			case TokenType.Division:
				return "/";
			case TokenType.BitwiseNot:
				return "~";
			case TokenType.IncrementConcatenate:
				return "++";
			case TokenType.Decrement:
				return "--";
			case TokenType.EqualTo:
				return "==";
			case TokenType.GreaterThan:
				return ">";
			case TokenType.LessThan:
				return "<";
			case TokenType.GreaterThanOrEqualTo:
				return ">=";
			case TokenType.LessThanOrEqualTo:
				return "<=";
			case TokenType.BitwiseOr:
				return "|";
			case TokenType.OpenBracket:
				return "[";
			case TokenType.CloseBracket:
				return "]";
		}

		return tokenType.ToString();
	}
}