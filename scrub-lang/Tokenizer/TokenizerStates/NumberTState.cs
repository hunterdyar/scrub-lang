using System.Text;
using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Tokenizer;
//todo: allow hex, octal, and binary literal values.
public class NumberTState(Tokenizer context) : TokenizerStateBase(context)
{
	public enum NumberLiteralType
	{
		Unknown,
		Regular,//base 10
		Binary0b,
		HexOx,
		Octal0o,
	}

	public NumberLiteralType LiteralType => _literalType;
	private NumberLiteralType _literalType = NumberLiteralType.Unknown;
	
	private StringBuilder numberLiteral = new();
	private string prefix = "";
	
	public override void Consume(char c, Location loc)
	{
		//first character has to be a digit, but then it can be a b (0b0001) or x or o; or a d (32d is 32 forced type to double)
		//we also could suppport _'s and so on, like some ways of writing numbers. i'm more interested in supporting binary, hex, octal tho.
		if (prefix == "" && c == '0')
		{
			prefix = "0";
			numberLiteral.Append(c);
			return;//don't exit state.
		}else if (prefix == "0")
		{
			if (c == 'x')
			{
				prefix = prefix + c;
				_literalType = NumberLiteralType.HexOx;
				numberLiteral.Clear();
				return;
			}else if (c == 'b')
			{
				_literalType = NumberLiteralType.Binary0b;
				prefix = prefix + c;
				numberLiteral.Clear();
				return;
			}else if (c == 'o')
			{
				_literalType = NumberLiteralType.Octal0o;
				prefix = prefix + c;
				numberLiteral.Clear();
				return;
			}
			else
			{
				_literalType = NumberLiteralType.Regular;
			}
		}

		if (_literalType == NumberLiteralType.HexOx)
		{
			if (char.IsDigit(c) || char.IsBetween(c, 'a', 'f') || char.IsBetween(c, 'A', 'F'))
			{
				numberLiteral.Append(c);
				return;
			}
			else
			{
				var lit = numberLiteral.ToString();
				context.AddToken(new Token(TokenType.HexLiteral, lit, loc));
				context.ExitState(this);
				context.ConsumeNext(c, loc);
				return;
			}
		}else if (_literalType == NumberLiteralType.Binary0b)
		{
			if (c == '0' || c == '1')
			{
				numberLiteral.Append(c);
				return;
			}
			else
			{
				var lit = numberLiteral.ToString();
				context.AddToken(new Token(TokenType.BinaryLiteral, lit, loc));
				context.ExitState(this);
				context.ConsumeNext(c, loc);
				return;
			}
		}else if (_literalType == NumberLiteralType.Octal0o)
		{
			if (char.IsBetween(c,'0','7'))
			{
				numberLiteral.Append(c);
				return;
			}
			else
			{
				var lit = numberLiteral.ToString();
				context.AddToken(new Token(TokenType.OctalLiteral, lit, loc));
				context.ExitState(this);
				context.ConsumeNext(c, loc);
				return;
			}
		}
		
		if (char.IsDigit(c) || char.IsNumber(c) || c == '.')
		{
			numberLiteral.Append(c);
			if (numberLiteral.Length > 1 || c != '0')
			{
				_literalType = NumberLiteralType.Regular;
			}
			return;
		}
		else
		{
			string s = numberLiteral.ToString();
			if (s.Contains("."))
			{
				var lit = numberLiteral.ToString();
				context.AddToken(new Token(TokenType.NumberLiteral, lit, loc));
				context.ExitState(this);
				context.ConsumeNext(c, loc);
				return;
			}
			else
			{
				var lit = numberLiteral.ToString();
				context.AddToken(new Token(TokenType.NumberLiteral, lit, loc));
				context.ExitState(this);
				context.ConsumeNext(c,loc);
				return;
			}
		}
	}
}