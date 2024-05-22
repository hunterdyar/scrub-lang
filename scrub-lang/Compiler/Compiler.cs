using System.Reflection.Metadata.Ecma335;
using System.Text;
using scrub_lang.Evaluator;
using scrub_lang.Objects;
using scrub_lang.Parser;
using scrub_lang.Tokenizer.Tokens;
using ConditionalExpression = scrub_lang.Parser.ConditionalExpression;
using Environment = scrub_lang.Evaluator.Environment;
using Object = scrub_lang.Objects.Object;
using String = scrub_lang.Objects.String;

namespace scrub_lang.Compiler;

public class Compiler
{
	private SymbolTable symbolTable = new SymbolTable();
	private List<Object> constants = new List<Object>();//ScrubObject?
	//todo; replace this with a ring buffer. AMong other reasons the naming is more clear.

	public List<CompilationScope> Scopes = new List<CompilationScope>();//todo: Replace with Stack<compscope>
	private int _scopeIndex = 0;
	public CompilationScope CurrentScope => Scopes[_scopeIndex];
	
	public Compiler()
	{
		CompilationScope cs = new CompilationScope();
		cs.Instuctions = new List<byte>();
		Scopes.Add(cs);
		_scopeIndex = 0;
	}
	public Compiler(Environment env)
	{
		if (env.SymbolTable != null)
		{
			symbolTable = env.SymbolTable;
		}

		if (env.Constants != null)
		{
			constants = env.Constants;
		}

		CompilationScope cs = new CompilationScope();
		cs.Instuctions = new List<byte>();
		Scopes.Add(cs);
		_scopeIndex = 0;
	}

	public Environment Environment()
	{
		var e = new Environment()
		{
			SymbolTable = symbolTable,
			Constants = constants
		};
		return e;
	}
	public ScrubCompilerError? Compile(IExpression expression)
	{
		if (expression is ProgramExpression pe)
		{
			foreach (var e in pe.Expressions)
			{
				var error = Compile(e);
				if (error != null)
				{
					return new ScrubCompilerError(error.Message);
				}

				//clean up the stack.
				if (e.ReturnsValue)
				{
					//if expression is "expression" and not "statement", then pop or not?
					Emit(OpCode.OpPop);
				}
				//these values are unused. All exresssions are unused unless they go into some operator or such.
				//it's a little weird because everything is an expression, but... after what would be a statement, we clean up the leftover value that the expressions created.
			}

			//Emit(OpCode.OpPop);
			return null;
		}else if (expression is ExpressionGroupExpression block)
		{
			for (int i = 0; i < block.Expressions.Count; i++)
			{
				var err = Compile(block.Expressions[i]);
				if (err != null)
				{
					return err;
				}

				if (i < block.Expressions.Count - 1)
				{
					//remove the value from the expression we just called.... which might not have a value? hmmm. shite.
					//our expression leaves us with one nice value at the end, which is what expression blocks become: their last value.
					//we also could always emit, and then (if needed?) removeLastPop, like with conditionals? 
					Emit(OpCode.OpPop);
				}
			}

			return null;
		}else if (expression is AssignExpression assignExpression)
		{
			//compile error.
			var err = Compile(assignExpression.Value);
			if (err != null)
			{
				return err;
			}

			if (LastInstructionIs(OpCode.OpPop))
			{
				RemoveLastInstruction();
			}

			if (symbolTable.TryResolve(assignExpression.Identifier.Identifier, out var symbol))
			{
				//assign
				Emit(OpCode.OpSetGlobal, symbol.Index);//assign
				Emit(OpCode.OpGetGlobal, symbol.Index);//return the value we assigned as the result of the expression.
				return null;
			}
			else
			{
				//This symbol does not exist yet. Creating it.
				symbol = symbolTable.Define(assignExpression.Identifier.Identifier);
				Emit(OpCode.OpSetGlobal, symbol.Index);
				Emit(OpCode.OpGetGlobal, symbol.Index);
				return null;
			}
			//return null			
		}else if (expression is IdentifierExpression identExpr)
		{
			if (symbolTable.TryResolve(identExpr.Identifier, out var symbol))
			{
				Emit(OpCode.OpGetGlobal, symbol.Index);
			}
			else
			{
				return new ScrubCompilerError($"Undefined Variable {identExpr.Identifier}");
			}

			return null;
		}
		
		else if (expression is BinaryOperatorExpressionBase bin)
		{
			if (bin.Operator == TokenType.LessThan)
			{
				var rightErr = Compile(bin.Right);
				if (rightErr != null)
				{
					return new ScrubCompilerError(rightErr.Message);
				}
				
				var leftErr = Compile(bin.Left);
				if (leftErr != null)
				{
					return new ScrubCompilerError(leftErr.Message);
				}

				// a<b gets sent to the stack as b>a. we do right before left (above), then a > op
				Emit(OpCode.OpGreaterThan);
				return null;
			}
			var leftError = Compile(bin.Left);
			if (leftError != null)
			{
				return new ScrubCompilerError(leftError.Message);
			}

			var rightError = Compile(bin.Right);
			if (rightError != null)
			{
				return new ScrubCompilerError(rightError.Message);
			}

			switch (bin.Operator)
			{
				case TokenType.Plus:
					Emit(OpCode.OpAdd);
					break;
				case TokenType.Minus:
					Emit(OpCode.OpSubtract);
					break;
				case TokenType.Multiply:
					Emit(OpCode.OpMult);
					break;
				case TokenType.Division:
					Emit(OpCode.OpDivide);
					break;
				case TokenType.EqualTo:
					Emit(OpCode.OpEqual);
					break;
				case TokenType.NotEquals:
					Emit(OpCode.OpNotEqual);
					break;
				case TokenType.GreaterThan:
					Emit(OpCode.OpGreaterThan);
					break;
				default:
					return new ScrubCompilerError($"Unable to Compile Operator {Token.OperatorToString(bin.Operator)}");
			}

			return null;
		}else if (expression is PrefixExpression pre)
		{
			var error = Compile(pre.Right);//should leave an integer, true, or false on the stack.
			if (error != null)
			{
				return new ScrubCompilerError(error.Message);
			}

			switch (pre.Op)
			{
				case TokenType.Minus:
					Emit(OpCode.OpNegate);
					break;
				case TokenType.Bang:
					Emit(OpCode.OpBang);
					break;
				default:
					return new ScrubCompilerError($"Unable to Compile Prefix Operator {pre.Op}");
			}

			return null;
		}else if (expression is PostfixExpressionBase post)
		{
			var leftError = Compile(post.Left);
			if (leftError != null)
			{
				return new ScrubCompilerError(leftError.Message);
			}

			return null;
		}
		//other base expressions...
		
		//code-flow
		else if (expression is ConditionalExpression condExpr)
		{
			//conditional (stack is true or false)
			//jump if false to :after-consequence
			//consequence(and we leave a value in it, so don't pop to nothing)
			//jump to :skip-alternative
			//:after-consequence
			//alternative
			//:skip-alternative
			
			var err = Compile(condExpr.Conditional);
			if (err != null)
			{
				return err;
			}

			var jumpNqePos = Emit(OpCode.OpJumpNotTruthy, 9999);//bogus value, let's fix it later.

			err = Compile(condExpr.Consequence);
			if (err != null)
			{
				return err;
			}
			
			// //not using this as a return value, so pop it? or dont?
			// Emit(OpCode.OpPop);
			if (LastInstructionIs(OpCode.OpPop))
			{
				RemoveLastInstruction();
			}

			var jumpPos = Emit(OpCode.OpJump, 99999);
			var afterConsequencePos = CurrentScope.Instuctions.Count;
			ChangeOperand(jumpNqePos, afterConsequencePos);

			//Update the jump-if-conditional-is-not-true destination to be the destination after we compiled the consequence.
			//if there is an alternative, then the true path needs to hit a jump to skip it.
			//and the false path needs to jump to after that jump. lest we skip the whole thing.
			if (condExpr.Alternative is NullExpression)//==nulls
			{
				//THis... could be just the compile of the alternative.... right? null just emits a null always? todo
				//we are't compiling the alternative here, so if nothing else, it's a minor optimization.
				Emit(OpCode.OpNull);
			}
			else
			{
				err = Compile(condExpr.Alternative);
				if (err != null)
				{
					return err;
				}

				if (LastInstructionIs(OpCode.OpPop))
				{
					RemoveLastInstruction();
				}
			}

			var afterAlternativePos = CurrentScope.Instuctions.Count;
			ChangeOperand(jumpPos, afterAlternativePos); //backpatch to fix the bogus value.
			
			return null;
		}
		//should we have a literalExpressionBase?
		else if (expression is NumberLiteralExpression numLitExp)
		{
			//todo: we are only supporting int's to start.
			var number = numLitExp.GetScrubObject();
			//create a new instruction. The operatand is the index of number in our constants pool, basically.
			Emit(OpCode.OpConstant, AddConstant(number));
			return null;
		}else if (expression is BoolLiteralExpression boolLitExp)
		{
			Emit(boolLitExp.Literal ? OpCode.OpTrue : OpCode.OpFalse);
			return null;
		}else if(expression is NullExpression)
		{
			Emit(OpCode.OpNull);
			//chill.
			return null;
		}else if (expression is StringLiteralExpression stringlitExpr)
		{
			var str = stringlitExpr.GetScrubObject();
			Emit(OpCode.OpConstant, AddConstant(str));
			return null;
		}else if (expression is ArrayLiteralExpression arrayLiteralExpression)
		{
			int length = arrayLiteralExpression.Values.Count;
			for (int i = 0; i < length; i++)
			{
				var err = Compile(arrayLiteralExpression.Values[i]);
				if (err != null)
				{
					return err;
				}
			}

			Emit(OpCode.OpArray, length);
			return null;
		}else if (expression is IndexExpression arrayLookupExpression)
		{
			//first put the array on the stack.
			var err = Compile(arrayLookupExpression.Left);
			if (err != null)
			{
				return err;
			}
			//put the index on the stack.
			err = Compile(arrayLookupExpression.Index);
			if (err != null)
			{
				return err;
			}

			Emit(OpCode.OpIndex);
			return null;
		}else if (expression is FunctionDeclarationExpression fde)
		{
			var args = fde.Arguments;
			
			EnterScope();
			var err = Compile(fde.Expression);
			if (err != null)
			{
				return err;
			}

			//again I think I bypass this case that Monkey has, because everything is an expression.
			if (LastInstructionIs(OpCode.OpPop))
			{
				Console.WriteLine("Warning: Function ended with a pop.");
				//there's a more optimized way to do this in one, using replaceInstruction and also replace the opcode.
				RemoveLastInstruction();
				Emit(OpCode.OpReturnValue);
			}

			if (!LastInstructionIs(OpCode.OpReturnValue))
			{
				//Console.WriteLine("Warning: Function didn't return a value. That's a problemo!");
				//Emit(OpCode.OpNull);
				Emit(OpCode.OpReturnValue);
			}
			//Add a function literal direclty onto the stack.
			var instructions = LeaveScope();
			var compiledFunction = new Function(instructions);
			Emit(OpCode.OpConstant, AddConstant(compiledFunction));
			return null;
		}else if (expression is ReturnExpression rete)
		{
			var err = Compile(rete.ReturnValue);
			if (err != null)
			{
				return err;
			}
			//presumably, the return value is now on the stack. If there wasn't one, it's null.
			Emit(OpCode.OpReturnValue);
			return null;
		}else if (expression is CallExpression callExpr)
		{
			var err = Compile(callExpr.Expression);
			//if this is the identifier, it will leave it's data on the stack... which is the instructions to call.
			if (err != null)
			{
				return err;
			}
			
			Emit(OpCode.OpCall);
			return null;
		}

		if (expression == null)
		{
			return Compile(new NullExpression());
			return new ScrubCompilerError("Expression is null. The actual native null, I mean.");
			//todo: Warnings and such here?
		}
		
		//Unhandled Expression Type
		StringBuilder sb = new StringBuilder();
		expression.Print(sb);
		return new ScrubCompilerError($"Unable to compile expression {sb}. Probably not implemented the type yet.");
	}

	public int Emit(OpCode op, params int[] operands)
	{
		var ins = Op.Make(op, operands);
		var pos = AddInstruction(ins);
		SetLastInstruction(op, pos);
		return pos;
	}

	private void SetLastInstruction(OpCode op, int pos)
	{
		var previous = CurrentScope.LastInstruction;
		var last = new EmittedInstruction(op, pos);
		CurrentScope.PreviousInstruction = previous;
		CurrentScope.LastInstruction = last;
	}

	public int AddInstruction(byte[] instruction)
	{
		var posNewInstruction = CurrentScope.Instuctions.Count;
		foreach (byte b in instruction)
		{
			CurrentScope.Instuctions.Add(b);
		}

		return posNewInstruction;
	}

	private void EnterScope()
	{
		var s = new CompilationScope();
		Scopes.Add(s);
		_scopeIndex++;
	}

	private byte[] LeaveScope()
	{
		var instructions = CurrentScope.Instuctions.ToArray();
		Scopes.RemoveAt(_scopeIndex);
		_scopeIndex--;
		return instructions;
	}
	/// <returns>Returns this constants insdex in the constants pool.</returns>
	public int AddConstant(Object obj)
	{
		this.constants.Add(obj);
		return this.constants.Count - 1;
	}

	private bool LastInstructionIs(OpCode op)
	{
		if (CurrentScope.Instuctions.Count == 0)
		{
			return false;
		}
		return CurrentScope.LastInstruction.Op == op;
	}

	private void RemoveLastInstruction()
	{
		CurrentScope.Instuctions = CurrentScope.Instuctions.Slice(0, CurrentScope.LastInstruction.Position);
		CurrentScope.LastInstruction = CurrentScope.PreviousInstruction;
	}

	private void ReplaceInstruction(int pos, byte[] newInstruction)
	{
		for (int i = 0; i < newInstruction.Length; i++)
		{
			CurrentScope.Instuctions[pos + i] = newInstruction[i];
		}
	}

	private void ChangeOperand(int opPos, int operand)
	{
		//we assume that we only change op's of the same type.
		var op = (OpCode)CurrentScope.Instuctions[opPos];
		var newInstruction = Op.Make(op, operand);
		
		ReplaceInstruction(opPos, newInstruction);
	}
	//this is what gets passed to the VM.
	public ByteCode ByteCode()
	{
		return new ByteCode(CurrentScope.Instuctions.ToArray(),constants.ToArray());
	}
	
}