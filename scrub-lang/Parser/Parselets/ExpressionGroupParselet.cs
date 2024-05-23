﻿using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Parser.Parselets;

public class ExpressionGroupParselet : IPrefixParselet
{
	private List<IExpression> _expressions = new List<IExpression>();
	public IExpression Parse(Parser parser, Token token)
	{
		_expressions.Clear();
		while (!parser.Peek(TokenType.EndExpressionBlock) && !parser.Peek(TokenType.EOF))
		{
			var e = parser.ParseExpression(0);
			_expressions.Add(e);
		}

		parser.Consume(TokenType.EndExpressionBlock);

		return new ExpressionGroupExpression(_expressions);
	}
}