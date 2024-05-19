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
	VarKeyword,
	FunctionKeyword,
	EndExpression,//semicolon
	StartExpressionBlock,
	EndExpressionBlock,
	Unexpected,
}