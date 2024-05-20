﻿using System.Text;

namespace scrub_lang.Parser;

//so you typed some number into the program, huh?

public class NumberLiteralExpression : IExpression
{
	public string Literal => _literal;
	private readonly string _literal;
	public bool IsInt => !_isDouble && _isInt;
	public bool IsDouble => _isDouble;//if it's a double, it must not be anything else!
	//when is it a uint implicitly? never? we need some kind of "float 3f" or "uint u23" or OxFF" and such. 
	private bool _isInt;
	public int AsInt => _litInt;
	private int _litInt;

	private bool _isDouble;
	public double AsDouble => _litDouble;
	private double _litDouble;
	
	//problem: - is an operator, so it will never be negative. unsigned will have to be a special case (like Ob)
	private bool _isUint;
	public uint AsUint => _litUint;
	private uint _litUint;
	
	//todo: allow hex, octal, and binary literal values.

	public NumberLiteralExpression(string literal)
	{
		_isInt = int.TryParse(literal, out _litInt);
		_isUint = uint.TryParse(literal, out _litUint);
		_isDouble = double.TryParse(literal, out _litDouble);
		this._literal = literal;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append(_literal);
	}
}