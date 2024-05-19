namespace scrub_lang.Tokenizer.Tokens;

public enum TokenType
{
	EOF,
	Identifier,
	NumberLiteral,
	Assignment,
	Plus,
	Minus,
	Multiply,
	PowerOf,
	Modulo,
	Division,
	NotEquals,
	EqualTo,
	Not,
	OpenParen,
	CloseParen,
	GreaterThan,
	GreaterThanOrEqualTo,
	LessThan,
	LessThanOrEqualTo,
	String,
	IfKeyword,
	WhileKeyword,
	EndExpression,//semicolon
	StartExpressionBlock,
	EndExpressionBlock,
	Unexpected,
}