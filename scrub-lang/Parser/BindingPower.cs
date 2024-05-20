namespace scrub_lang.Parser;

public static class BindingPower
{
	//this but flipped around...
	//https://en.cppreference.com/w/c/language/operator_precedence
	public static readonly int Comma = 1; // ,

	public static readonly int AssignmentBySum = 4;//+=, -=
	public static readonly int Assignment = 4;// =
	public static readonly int Ternery = 5;//?=
	public static readonly int LogicalOr = 6;// ||
	public static readonly int LogicalAnd = 7;// &&
	public static readonly int BitwiseOr = 8;// |
	public static readonly int BitwiseXor = 9; // ^
	public static readonly int BitwiseAnd = 10;//&
	
	public static readonly int Equality = 11;//==, !=
	public static readonly int NumericCompare = 12;// >, <, >=, <=
	public static readonly int BitwiseShift = 13;// >> <<
	public static readonly int Sum = 14; //+ -
	
	public static readonly int Product = 15; // * /
	public static readonly int Modulo = 15; //%
	
	public static readonly int Exponent = 16; // **
	public static readonly int Prefix = 17; // -a genericly
	public static readonly int PostFix = 18; // a++ genericly

	public static readonly int BitwiseNot = 19;
	public static readonly int UnarySum = 19;// -a
	public static readonly int Not = 19;// !
	public static readonly int Cast = 19;//what would a cast even be in scrub.
	// private static readonly int 

	public static readonly int MemberAccess = 20;// .
	public static readonly int ArraySubscript = 20;// []
	public static readonly int Call = 20;// f()
	public static readonly int Increment = 20; //++

}