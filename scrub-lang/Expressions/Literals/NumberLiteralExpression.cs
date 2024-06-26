﻿using System.Text;
using scrub_lang.Compiler;
using scrub_lang.Evaluator;
using scrub_lang.Objects;
using scrub_lang.Tokenizer;

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
	public Location Location { get; }
	
	public NumberLiteralExpression(string literal, Location location, int baseVal = 10)
	{
		Location = location;
		this._literal = literal;
		if (baseVal == 10)
		{
			_isInt = int.TryParse(literal, out _litInt);
		}
		else
		{
			_litInt = Convert.ToInt32(literal, baseVal);
			_isInt = true;
		}

	}

	public void Print(StringBuilder sb)
	{
		sb.Append(_literal);
	}

	public Objects.Object GetScrubObject()
	{
		if (_isDouble)
		{
			//return double
		}

		if (_isInt)
		{
			return new Integer(AsInt);
		}

		return new ScrubCompilerError($"Unable to Convert '{_literal}' into Object");
	}
}