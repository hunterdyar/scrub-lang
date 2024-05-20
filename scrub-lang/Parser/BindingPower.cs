namespace scrub_lang.Parser;

public static class BindingPower
{
	public static readonly int Assignment = 1;
	public static readonly int Conditional = 2;
	public static readonly int Sum = 3;
	public static readonly int Product = 4;
	public static readonly int Exponent = 5;
	public static readonly int Prefix = 6;
	public static readonly int PostFix = 7;
	public static readonly int Call = 8;
}