﻿using System.Text;

namespace scrub_lang.Parser;

/// <summary>
/// a ? b : c
/// or if(a){b}else{c}
/// </summary>
public class ConditionalExpression : IExpression
{
	private IExpression _conditionExpr;
	private IExpression _thenExpr;
	private IExpression _elseExpr;

	public ConditionalExpression(IExpression conditionExpr, IExpression thenExpr, IExpression elseExpr)
	{
		_conditionExpr = conditionExpr;
		_thenExpr = thenExpr;
		_elseExpr = elseExpr;
	}

	public void Print(StringBuilder sb)
	{
		sb.Append('(');
		_conditionExpr.Print(sb);
		sb.Append(" ? ");
		_thenExpr.Print(sb);
		sb.Append(" : ");
		_elseExpr.Print(sb);
		sb.Append(')');
	}
}