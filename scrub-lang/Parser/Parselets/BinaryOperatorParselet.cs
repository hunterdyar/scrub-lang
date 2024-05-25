using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class BinaryOperatorParselet : IInfixParselet
{
	private readonly int _bindingPower;
	private readonly bool _isRight;

	public BinaryOperatorParselet(int bindingPower, bool isRight)
	{
		_bindingPower = bindingPower;
		_isRight = isRight;
	}

	public IExpression Parse(Parser parser, IExpression left, Token token)
	{
		//right hand side slightly lower so that ^binary and 2^3 work correctly.
		IExpression right = parser.ParseExpression(_bindingPower - (_isRight ? 1 : 0));
		if (BinaryMathExpression.IsBinaryMathOperator(token.TokenType))
		{
			return new BinaryMathExpression(left, token.TokenType, right, token.Location);
		}else if (BinaryBitwiseExpression.IsBinaryBitwiseOperator(token.TokenType))
		{
			return new BinaryBitwiseExpression(left, token.TokenType, right, token.Location);
		}else if (BinaryConditionalExpression.IsBinaryConditionalOperator(token.TokenType))
		{
			return new BinaryConditionalExpression(left, token.TokenType, right,token.Location);
		}

		throw new ParseException($"Cannot parse {token.Literal} as binary operator at {token.Location}. ");
	}

	public int GetBindingPower()
	{
		return _bindingPower;
	}
}