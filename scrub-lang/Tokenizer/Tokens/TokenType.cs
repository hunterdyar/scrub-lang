﻿namespace scrub_lang.Tokenizer.Tokens;

public enum TokenType
{
	EOF,
	Identifier,
	NumberLiteral,
	Assignment,
	Plus,
	Increment,
	Decrement,
	Minus,
	Multiply,
	PowerOfXOR,//^
	Modulo,
	Division,
	NotEquals,
	EqualTo,
	And,
	Or,
	BitwiseOr,
	BitwiseAnd,
	BitwiseNot,
	BitwiseLeftShift,
	BitwiseRightShift,
	Bang,
	Question,
	Colon,
	Comma,
	OpenParen,
	CloseParen,
	GreaterThan,
	GreaterThanOrEqualTo,
	LessThan,
	LessThanOrEqualTo,
	String,
	IfKeyword,
	WhileKeyword,
	FunctionKeyword,
	TrueKeyword,
	FalseKeyword,
	EndExpression,//semicolon
	StartExpressionBlock,
	EndExpressionBlock,
	Unexpected,
}