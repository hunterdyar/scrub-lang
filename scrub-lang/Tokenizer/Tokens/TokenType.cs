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
	PowerOf,// **
	BitwiseXOR, //^
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
	ElseKeyword,
	WhileKeyword,
	FunctionKeyword,
	TrueKeyword,
	FalseKeyword,
	ReturnKeyword,
	NullKeyword,
	EndExpression,//semicolon... or line break?
	StartExpressionBlock,
	EndExpressionBlock,
	Unexpected,
}