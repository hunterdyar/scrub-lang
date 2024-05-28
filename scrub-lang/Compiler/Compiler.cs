using System.Text;
using scrub_lang.Evaluator;
using scrub_lang.Objects;
using scrub_lang.Parser;
using scrub_lang.Tokenizer.Tokens;
using ConditionalExpression = scrub_lang.Parser.ConditionalExpression;
using Environment = scrub_lang.Evaluator.Environment;
using Object = scrub_lang.Objects.Object;

namespace scrub_lang.Compiler;

public class Compiler
{
	private SymbolTable _symbolTable = new SymbolTable();
	private List<Object> _constants = new List<Object>();//ScrubObject?
	public Stack<CompilationScope> Scopes = new Stack<CompilationScope>();
	private int _scopeIndex = 0;//we don't need scope index now that scopes is a stack; but it's useful to glance at when debugging.
	public CompilationScope CurrentScope => Scopes.Peek();
	
	public Compiler()
	{
		CompilationScope cs = new CompilationScope();
		cs.Instructions = new List<int>();
		Scopes.Push(cs);
		_scopeIndex = 0;
		DefineBuiltins();
	}

	public bool TryCompile(IExpression root, out ScrubCompilerError? error, out Program? program)
	{
		var r = Compile(root);
		if (r != null)
		{
			error = r;
			program = null;
			return false;
		}

		error = null;
		program = GetProgram();
		return true;

	}
	public Compiler(Environment env)
	{
		if (env.SymbolTable != null)
		{
			_symbolTable = env.SymbolTable;
		}

		if (env.Constants != null)
		{
			_constants = env.Constants;
		}

		CompilationScope cs = new CompilationScope();
		cs.Instructions = new List<int>();
		Scopes.Push(cs);
		_scopeIndex = 0;
		DefineBuiltins();
	}
	
	public Environment Environment()
	{
		var e = new Environment()
		{
			SymbolTable = _symbolTable,
			Constants = _constants
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

				Emit(e.Location,OpCode.OpPop);
				//these values are unused. All exresssions are unused unless they go into some operator or such.
				//it's a little weird because everything is an expression, but... after what would be a statement, we clean up the leftover value that the expressions created.
			}
			return null;
		}else if (expression is ExpressionGroupExpression block)
		{
			//an expression group takes a sequence of expressions and returns a single value from them.
			if (block.Expressions.Length == 0)
			{
				Emit(block.Location,OpCode.OpNull);
			}else if (block.Expressions.Length == 1)
			{
				
				var err = Compile(block.Expressions[0]);
				if (err != null)
				{
					return err;
				}

				if (block.Expressions[0] is ConditionalExpression)
				{
					Console.WriteLine($"Conditional is only item in expression block. Last: {CurrentScope.LastInstruction.Op}");
				}

				return null;
			}
			else
			{
				for (int i = 0; i < block.Expressions.Length; i++)
				{
					
					var err = Compile(block.Expressions[i]);
					if (err != null)
					{
						return err;
					}

					//Emit(OpCode.OpPop);//emitting always made 0 difference (update: WERONG itmatters)
					if (i < block.Expressions.Length - 1)
					{
						
						//remove the value from the expression we just called.... which might not have a value? hmmm. shite.
						//our expression leaves us with one nice value at the end, which is what expression blocks become: their last value.
						//we also could always emit, and then (if needed?) removeLastPop, like with conditionals? 
						Emit(block.Expressions[i].Location,OpCode.OpPop);
						
					}else{
						if (block.Expressions[i] is ConditionalExpression)
						{
							Console.WriteLine(
								$"Conditional is last expression block. Last: {CurrentScope.LastInstruction.Op} ");
						}
					}
				}
			}

			return null;
		}else if (expression is FunctionDeclarationExpression funcDef)
		{
			//todo: consider re-combining functiondecs and assign, since the code is duplicated.
			//create the symbol before compiling the function, so that it can recursively find its own name.
			Symbol functionNameSymbol;
			bool isNotNewSymbol = _symbolTable.TryResolve(funcDef.Identity.Identifier, out functionNameSymbol);

			//we compiled a function
			var err = Compile(funcDef.Function);
			if (err != null)
			{
				return err;
			}
			//anyway

			if (LastInstructionIs(OpCode.OpPop))
			{
				RemoveLastInstruction();
			}

			if (isNotNewSymbol)
			{
				return new ScrubCompilerError($"A function named {functionNameSymbol.Name} has already been defined.");
				functionNameSymbol = _symbolTable.Define(funcDef.Identity.Identifier);
			}

			//the only difference between this and AssignExpression is that functions defined as func NAME(){} are always global scoped. thats... weird?s
			if (isNotNewSymbol)
			{
				if (functionNameSymbol.Scope == ScopeDef.Global)
				{
					Emit(funcDef.Location,OpCode.OpSetGlobal, functionNameSymbol.Index); //assign
				}
				else
				{
					//else local scope
					Emit(funcDef.Location,OpCode.OpSetLocal, functionNameSymbol.Index);
				}

				return null;
			}
			else
			{
				//This function name does not exist yet. Creating it.
				functionNameSymbol = _symbolTable.Define(funcDef.Identity.Identifier);
				if (functionNameSymbol.Scope == ScopeDef.Global)
				{
					Emit(funcDef.Location,OpCode.OpSetGlobal, functionNameSymbol.Index);
				}
				else
				{
					//else local scope.
					Emit(funcDef.Location,OpCode.OpSetLocal, functionNameSymbol.Index);
				}

				return null;
			}
		}
		else if (expression is AssignExpression assignExpression)
		{
			bool resolved = _symbolTable.TryResolve(assignExpression.Identifier.Identifier, out var symbol);
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

			//only check the current scope on assignment. we need some override for global access.
			if(resolved){
				//assign (overwrite)
				if (symbol.Scope == ScopeDef.Global)
				{
					Emit(assignExpression.Location,OpCode.OpSetGlobal, symbol.Index); //assign
				}
				else
				{
					//else local scope
					Emit(assignExpression.Location,OpCode.OpSetLocal, symbol.Index);
				}

				return null;
			}
			else
			{
				//This symbol does not exist yet. Creating it.
				symbol = _symbolTable.Define(assignExpression.Identifier.Identifier);
				if (symbol.Scope == ScopeDef.Global)
				{
					//location is this or identifier?
					Emit(assignExpression.Location,OpCode.OpSetGlobal, symbol.Index);
				}
				else
				{
					//else local scope.
					Emit(assignExpression.Location,OpCode.OpSetLocal, symbol.Index);
				}

				return null;
			}
			//return null			
		}else if (expression is IdentifierExpression identExpr)
		{
			if (_symbolTable.TryResolve(identExpr.Identifier, out var symbol))
			{
				EmitLoadSymbol(identExpr.Location,symbol);
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
				Emit(bin.Location,OpCode.OpGreaterThan);
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
					Emit(bin.Location,OpCode.OpAdd);
					break;
				case TokenType.Minus:
					Emit(bin.Location, OpCode.OpSubtract);
					break;
				case TokenType.Multiply:
					Emit(bin.Location, OpCode.OpMult);
					break;
				case TokenType.Division:
					Emit(bin.Location, OpCode.OpDivide);
					break;
				case TokenType.EqualTo:
					Emit(bin.Location, OpCode.OpEqual);
					break;
				case TokenType.NotEquals:
					Emit(bin.Location, OpCode.OpNotEqual);
					break;
				case TokenType.GreaterThan:
					Emit(bin.Location, OpCode.OpGreaterThan);
					break;
				case TokenType.BitwiseAnd:
					Emit(bin.Location, OpCode.OpBitAnd);
					break;
				case TokenType.BitwiseOr:
					Emit(bin.Location, OpCode.OpBitOr);
					break;
				case TokenType.BitwiseXOR:
					Emit(bin.Location, OpCode.OpBitXor);
					break;
				case TokenType.BitwiseLeftShift:
					Emit(bin.Location, OpCode.OpBitShiftLeft);
					break;
				case TokenType.BitwiseRightShift:
					Emit(bin.Location, OpCode.OpBitShiftRight);
					break;
				case TokenType.Modulo:
					Emit(bin.Location, OpCode.OpMod);
					break;
				case TokenType.PowerOf:
					Emit(bin.Location, OpCode.OpPow);
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
					Emit(pre.Location,OpCode.OpNegate);
					break;
				case TokenType.Bang:
					Emit(pre.Location,OpCode.OpBang);
					break;
				case TokenType.BitwiseNot:
					Emit(pre.Location,OpCode.OpBitNot);
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
			
			//if increment, increment, etc.
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
			//with portals:
			//Cond,jumpJNq[],consequence,jumptoend[],jumpJNQ_back[],alternative,JumpPastjumpback[],jumpback[]
			var err = Compile(condExpr.Conditional);
			if (err != null)
			{
				return err;
			}
			
			var jumpNqePos = Emit(condExpr.Conditional.Location,OpCode.OpJumpNotTruthy, -1,0);//-1 is bogus value, let's fix it later.
			err = Compile(condExpr.Consequence);
			if (err != null)
			{
				return err;
			}
			if (LastInstructionIs(OpCode.OpPop))
			{
				RemoveLastInstruction();
			}

			//skip alternative if you did consequence.
			var jumpPos = Emit(condExpr.Consequence.Location,OpCode.OpJump, -1,0);
			
			//this is the "catch" portal of the jumpnottruth, which jumps to after this. we jump to before it.
			//this is the jumpnottruthy for going back past the consequence on false, we move to the before consequence position
			Emit(condExpr.Consequence.Location,OpCode.OpJump, jumpNqePos,1);
			var afterConsequencePos = CurrentScope.Instructions.Count;
			if (Scopes.Count > 1)
			{
				afterConsequencePos += 1;
			}
			ChangeOperand(jumpNqePos, afterConsequencePos,0);//the plus one here is a pop added by something else? sometimes...

			//Update the jump-if-conditional-is-not-true destination to be the destination after we compiled the consequence.
			//if there is an alternative, then the true path needs to hit a jump to skip it.
			//and the false path needs to jump to after that jump. lest we skip the whole thing.
			if (condExpr.Alternative is NullExpression)//==nulls
			{
				//This could be just the compile of the alternative. null emits a null.
				//if nothing else, it's a minor optimization. (otherwise, null, pop, get rid of last pop)
				Emit(condExpr.Location,OpCode.OpNull);
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

			//skip past the jumps that jump us for going in reverse, when going forward through consequence.
			//Emit(OpCode.OpJump, CurrentScope.Instuctions.Count+1);//skip self and next? +2? 
			//curr+jumpx2
			//the opposite side of the JUMP command.... minus one. this is for going backwards, it must be skipped when going forwards
			//the -2 are the jumps we added
			Emit(condExpr.Alternative.Location,OpCode.OpJumpNotTruthy, afterConsequencePos-2,1);//problem: the truth/false position is one too far back?
			var afterAlternativePos = CurrentScope.Instructions.Count;
			if (Scopes.Count > 1)
			{
				//this just made all my tests pass and im kind of furious about it! 
				//todo: make jumps inside and outside of closures work the same.
				//or at least move this edge-case to the VM and not the compiler, so a section of code compiled inside and outside of a function will be the same.
				afterAlternativePos += 1;
			}
			// Console.WriteLine($"afterAlternativePos is after: {Op.InstructionToString(CurrentScope.Instructions[afterAlternativePos-1])}");
			ChangeOperand(jumpPos, afterAlternativePos,0); //this or this -1 is ... being tested.//backpatch to fix the bogus value.
			return null;
		}
		//should we have a literalExpressionBase?
		else if (expression is NumberLiteralExpression numLitExp)
		{
			var number = numLitExp.GetScrubObject();
			//create a new instruction. The operatand is the index of number in our constants pool, basically.
			Emit(numLitExp.Location,OpCode.OpConstant, AddConstant(number));
			return null;
		}else if (expression is BoolLiteralExpression boolLitExp)
		{
			Emit(boolLitExp.Location,boolLitExp.Literal ? OpCode.OpTrue : OpCode.OpFalse);
			return null;
		}else if(expression is NullExpression ne)
		{
			Emit(ne.Location,OpCode.OpNull);
			//chill.
			return null;
		}else if (expression is StringLiteralExpression stringlitExpr)
		{
			var str = stringlitExpr.GetScrubObject();
			Emit(stringlitExpr.Location,OpCode.OpConstant, AddConstant(str));
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

			Emit(arrayLiteralExpression.Location,OpCode.OpArray, length);
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

			Emit(arrayLookupExpression.Location,OpCode.OpIndex);
			return null;
		}else if (expression is FunctionLiteralExpression funcLiteralExpr)
		{
			EnterScope();
			string n = "";
			if (!string.IsNullOrEmpty(funcLiteralExpr.Name))
			{
				_symbolTable.DefineFunctionName(funcLiteralExpr.Name);
				n = funcLiteralExpr.Name;
			}
			
			var args = funcLiteralExpr.Arguments;
			foreach (var arg in args)
			{
				if (arg.Identifier == n)
				{
					return new ScrubCompilerError($"A function cannot have the same name as one of it's arguments! {arg.Location}");
				}
				_symbolTable.Define(arg.Identifier);
			}
			
			var err = Compile(funcLiteralExpr.Expression);
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
				Emit(funcLiteralExpr.Location,OpCode.OpReturnValue,0);
			}

			if (!LastInstructionIs(OpCode.OpReturnValue))
			{
				//Console.WriteLine("Warning: Function didn't return a value. That's a problemo!");
				//Emit(OpCode.OpNull);
				Emit(funcLiteralExpr.Location,OpCode.OpReturnValue,0);//return whatever we have.
			}
			var freeSymbols = _symbolTable.FreeTable;//we grab a reference to this before we leave the scope, and iterate over/load them them after. That's basically the point.
			var numLocals = _symbolTable.NumDefinitions;
			
			//Add a function literal direclty onto the stack.
			var instructions = LeaveScope();

			//put all free variables on the stack.
			foreach (var freeSymbol in freeSymbols)
			{
				EmitLoadSymbol(funcLiteralExpr.Location,freeSymbol);
			}
			
			var compiledFunction = new Function(instructions.Item1,args.Count, _symbolTable,instructions.Item2);
			
			var funcIndex = AddConstant(compiledFunction);
			Emit(funcLiteralExpr.Location,OpCode.OpClosure,funcIndex,freeSymbols.Count);//closures wrap functions. all functions are closures, even when there aren't any free variables.
			return null;
		}else if (expression is ReturnExpression rete)
		{
			var err = Compile(rete.ReturnValue);
			if (err != null)
			{
				return err;
			}
			
			if (this.Scopes.Count == 1)
			{
				return new ScrubCompilerError($"Invalid 'return' statement at root of program. Must be inside a function to return. {rete.Location}");
			}

			if (LastInstructionIs(OpCode.OpPop))
			{
				RemoveLastInstruction();
			}
			//presumably, the return value is now on the stack. If there wasn't one, it's null.
			Emit(rete.Location,OpCode.OpReturnValue);
			return null;
		}else if (expression is CallExpression callExpr)
		{
			var err = Compile(callExpr.Expression);
			//if this is the identifier, it will leave it's data on the stack... which is the instructions to call.
			if (err != null)
			{
				return err;
			}
			//push all of the values onto the stack after the function.
			foreach (var arg in callExpr.Args)
			{
				err = Compile(arg);
				if (err != null)
				{
					return err;
				}
			}
			
			Emit(callExpr.Location,OpCode.OpCall,callExpr.Args.Length);
			return null;
		}

		if (expression == null)
		{
			//return Compile(new NullExpression());
			return new ScrubCompilerError("Expression is null. The actual native null, I mean. We don't want that.");
		}
		
		//Unhandled Expression Type
		StringBuilder sb = new StringBuilder();
		expression.Print(sb);
		return new ScrubCompilerError($"Unable to compile expression {sb}. Probably not implemented the type yet.");
	}

	public int Emit(Location loc, OpCode op, params int[] operands)
	{
		var ins = Op.Make(op, operands);
		var pos = AddInstruction(loc,ins);
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

	public int AddInstruction(Location loc, int instruction)
	{
		var posNewInstruction = CurrentScope.AddInstruction(instruction,loc);
		return posNewInstruction;
	}

	private void EnterScope()
	{
		var s = new CompilationScope();
		Scopes.Push(s);
		_scopeIndex++;
		_symbolTable = _symbolTable.NewEnclosedSymbolTable();
	}

	private (int[],OpLocationLookup) LeaveScope()
	{
		var instructions = CurrentScope.Instructions.ToArray();
		Scopes.Pop();
		_scopeIndex--;
		_symbolTable = _symbolTable.Outer;
		return (instructions,CurrentScope.OpLocationLookup);
	}

	void EmitLoadSymbol(Location loc,Symbol s)
	{
		switch (s.Scope)
		{
			case ScopeDef.Global:
				Emit(loc,OpCode.OpGetGlobal, s.Index);
				break;
			case ScopeDef.Local:
				Emit(loc,OpCode.OpGetLocal, s.Index);
				break;
			case ScopeDef.Builtin:
				Emit(loc,OpCode.OpGetBuiltin, s.Index);
				break;
			case ScopeDef.Free:
				Emit(loc,OpCode.OpGetFree, s.Index);
				break;
			case ScopeDef.Function:
				Emit(loc,OpCode.OpCurrentClosure);//get constant... of a closure, but whatever the current one is in the vm at runtime.
				break;
			default:
				throw new CompileException($"what scope is {s.Scope}? that's not right.");
		}
	}
	
	/// <returns>Returns this constants insdex in the constants pool.</returns>
	public int AddConstant(Object obj)
	{ 
		//i think i need to overload the equality operator.
		var existing = _constants.FindIndex(x => obj.SameObjectData(x));
		if (existing!=-1)
		{
			return existing;
		}
		this._constants.Add(obj);
		return this._constants.Count - 1;
	}

	private bool LastInstructionIs(OpCode op)
	{
		if (CurrentScope.Instructions.Count == 0)
		{
			return false;
		}
		return CurrentScope.LastInstruction.Op == op;
	}

	private void RemoveLastInstruction()
	{
		CurrentScope.Instructions = CurrentScope.Instructions.Slice(0, CurrentScope.LastInstruction.Position);
		CurrentScope.LastInstruction = CurrentScope.PreviousInstruction;
	}

	private void ReplaceInstruction(int pos, int  newInstruction)
	{
		CurrentScope.Instructions[pos] = newInstruction;
	}

	private void ChangeOperand(int opPos,params int[] operand)
	{
		//we assume that we only change op's of the same type.
		var op = (OpCode)BitConverter.GetBytes(CurrentScope.Instructions[opPos])[0];
		var newInstruction = Op.Make(op, operand);
		
		ReplaceInstruction(opPos, newInstruction);
	}
	//this is what gets passed to the VM.
	/// <summary>
	/// gets bytecode, et al.
	/// </summary>
	public Program GetProgram()
	{
		//todo: decide how to encode the Location Lookup tables and pass them to the VM for errors.
		return new Program(CurrentScope.Instructions.ToArray(),_constants.ToArray(),CurrentScope.OpLocationLookup, _symbolTable);
	}

	private void DefineBuiltins()
	{
		for (int i = 0; i < Builtins.AllBuiltins.Length; i++)
		{
			_symbolTable.DefineBuiltin(i, Builtins.AllBuiltins[i].Name);
		}
	}
	
}