using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using scrub_lang.Compiler;

namespace scrub_lang.Compiler;

//this is the first time i've used this enum: type casting syntax and it's also probably the list time I ever will.
public enum OpCode: byte
{
	//regular operations (in terms of rewinding)
	OpPop = 0,
	OpAdd = 1,
	OpSubtract = 2,
	OpMult = 3,
	OpDivide = 4,
	OpBitAnd = 5,
	OpBitOr = 6,
	OpBitXor = 7,
	OpBitNot = 8,
	OpBitShiftLeft = 9,
	OpBitShiftRight = 10,
	OpTrue = 11,
	OpFalse = 12,
	OpNull = 13,
	OpEqual = 14,
	OpNotEqual = 15,
	OpGreaterThan = 16,
	OpBang = 17,
	OpNegate = 18,
	OpIndex = 19,
	OpConcat = 21, 

	//special ops.
	OpCall = 30,
	OpReturnValue = 31,
	OpCurrentClosure = 32,
	
	//jumps. while they are sandwichies, aren't sandwichies. They just work both ways. we handle them manually in the compiler.
	//[jmp][endpos][][][][][startpos][jmp]
	OpJump = 33,
	OpJumpNotTruthy = 34,
	
	//sandwichies. [code][operands][operands][code]
	OpConstant = 50,
	OpGetLocal = 52,
	OpSetLocal = 53,
	OpGetBuiltin = 54,
	OpClosure = 55,
	OpSetGlobal = 56,
	OpGetGlobal = 57,
	OpGetFree = 51, //get free! be free! Go forth! fly!
	OpArray = 58,
	}

public struct Definition
{
	public readonly string Name;
	public readonly int[] OperandWidths;//a number of operands, each of i bytes. 

	public Definition(string name, int[] widths)
	{
		Name = name;
		OperandWidths = widths;
	}

	//this is the opposite of Make()
	public (UInt16[], int) ReadOperands(byte[] instructions, int start)
	{
		var operands = new UInt16[this.OperandWidths.Length];
		int offset = start;//start of operands, not of operation.
		//x operands, each y units widw. For each operand...
		for (int i = 0; i < OperandWidths.Length; i++)
		{
			switch (OperandWidths[i]) // this operand is this many bytes.
			{
				case 0: break;
				case 1:
					operands[i] = (ushort)Op.ReadUInt8(instructions[offset]);
					break;
				case 2:
					operands[i] = Op.ReadUInt16([instructions[offset + 0], instructions[offset + 1]]);
					// if (BitConverter.IsLittleEndian)
					// {
					// 	operands[i] = BitConverter.ToInt16([instructions[offset + 1], instructions[offset]]);
					// }
					// else
					// {
					// 	operands[i] = BitConverter.ToInt16(instructions, offset);
					// }
					break;
			}

			offset += (UInt16)OperandWidths[i];
		}
		
		return (operands, offset-start);
	}
}

public static class Op
{
	private const int BidirectionalOpThreshold = 50;//if we end up with more than 50 "normal" op-codes, then we just increase it.
	public static Dictionary<OpCode, Definition> Definitions = new Dictionary<OpCode, Definition>()
	{
		{ OpCode.OpConstant, new Definition("Constant", new int[] { 2 }) },//index of const
		{ OpCode.OpPop, new Definition("Pop", new int[] { })},
		{ OpCode.OpAdd, new Definition("Add", new int[] { })},
		{ OpCode.OpMult, new Definition("Mult", new int[] { })},
		{ OpCode.OpSubtract, new Definition("Subtract", new int[] { })},
		{ OpCode.OpDivide, new Definition("Divide", new int[] { }) },
		{ OpCode.OpTrue, new Definition("True", new int[] { })},
		{ OpCode.OpFalse, new Definition("False", new int[] { })},
		{ OpCode.OpEqual, new Definition("equal", new int[] { }) },
		{ OpCode.OpNotEqual, new Definition("NotEqual", new int[] { }) },
		{ OpCode.OpGreaterThan, new Definition("GreaterThan", new int[] { }) },
		{ OpCode.OpNegate, new Definition("Negate", new int[] { }) },
		{ OpCode.OpBang, new Definition("Bang", new int[] { }) },
		{ OpCode.OpJump, new Definition("Jump", new int[] { 2 }) },//jump location
		{ OpCode.OpJumpNotTruthy, new Definition("JumpNotTruthy", new int[] { 2 }) },//jump location
		{ OpCode.OpNull, new Definition("NullPush", new int[] { }) },
		{ OpCode.OpSetGlobal, new Definition("SetGlobal", new int[] { 2 }) },//index of constant
		{ OpCode.OpGetGlobal, new Definition("GetGlobal", new int[] { 2 }) },//index of constant
		{ OpCode.OpConcat, new Definition("Concat", new int[] { }) },
		{ OpCode.OpArray, new Definition("Array", new int[] { 2 }) },//length of array
		{ OpCode.OpIndex, new Definition("Index", new int[] { }) },//no operands, it expects two values on the stack. an object and an index.
		{ OpCode.OpCall, new Definition("Call", new int[] { 1 }) },//number of arguments
		{ OpCode.OpReturnValue, new Definition("Return", new int[] { }) },//no arguments. The value to be returned will be on the stack.
		{ OpCode.OpGetLocal, new Definition("GetLocal", new int[] { 1 }) },//make these 2bytes? 256 local variables vs ... more than that.
		{ OpCode.OpSetLocal, new Definition("SetLocal", new int[] { 1 }) },
		{ OpCode.OpGetBuiltin, new Definition("GetBuiltin", new int[] { 1 }) },//if we ever have >256 builtins, then we can just make it two bytes wide.
		{ OpCode.OpClosure, new Definition("Closure",new int[]{2,1})},//constant index (2 bytes, matches OpConstant), 2nd is free variables count. 
		{ OpCode.OpGetFree, new Definition("GetFree", new int[] { 1 }) },//one operand of number variables
		{ OpCode.OpCurrentClosure, new Definition("CurrentClosure", new int[] { }) }, //OpClosure but for current scope. lets recursion happen.
		{ OpCode.OpBitAnd, new Definition("BitAnd", new int[] { }) },//binary AND,not conditional AND
		{ OpCode.OpBitOr, new Definition("BitOr", new int[] { }) },
		{ OpCode.OpBitXor, new Definition("BitXor", new int[] { }) },
		{ OpCode.OpBitNot, new Definition("BitNot", new int[] { }) },
		{ OpCode.OpBitShiftLeft, new Definition("BitShiftLeft", new int[] { }) },
		{ OpCode.OpBitShiftRight, new Definition("BitShiftRight", new int[] { }) },
	};

	public static bool IsOpCodeBidirectional(OpCode code)
	{
		return (int)code >= BidirectionalOpThreshold;
	}
	public static byte[] Make(OpCode op, params int[] operands)
	{
		if (!Definitions.TryGetValue(op, out var def))
		{
			return new byte[]{};
		}

		bool isOpCodeSandwich = IsOpCodeBidirectional(op);
		int instructionLength = isOpCodeSandwich ? 2 : 1;
		foreach (int width in def.OperandWidths)
		{
			instructionLength += width;
		}

		var instruction = new byte[instructionLength];
		instruction[0] = (byte)op;
		int offset = 1;
		for (int i = 0; i < operands.Length; i++)
		{
			var width = def.OperandWidths[i];
			switch (width)
			{
				case 0:
					break;
				case 1:
					instruction[offset] = ReadUInt8(operands[i]);
					break;
				case 2:
					var o = BitConverter.GetBytes(((ushort)operands[i]));
					//c# doesn't define endian-ness, so we have to check this flag.
					//note: MiscUtil EndianBitConverter.
					instruction[offset] = BitConverter.IsLittleEndian ? o[1] : o[0];
					instruction[offset + 1] = BitConverter.IsLittleEndian ? o[0] : o[1];
					break;
			}

			offset += width;
		}

		//OpCode Sandwich! Yee-haw!
		if (isOpCodeSandwich)
		{
			instruction[^1] = (byte)op;
		}

		return instruction;
	}

	public static UInt16 ReadUInt16(byte[] b)
	{
		if (BitConverter.IsLittleEndian)
		{
			return BitConverter.ToUInt16([b[1], b[0]]);
		}

		return BitConverter.ToUInt16(b);
	}

	public static byte ReadUInt8(byte[] b)
	{
		return (byte)b[0];
	}

	public static byte ReadUInt8(int b)
	{
		return (byte)b;
	}

	
	//This is the opposite of Make.
	
	//helper
	
	//minidissembler
	public static string InstructionsToString(byte[] instructions)
	{
		var sb = new StringBuilder();
		for (int i = 0; i < instructions.Length; i++)
		{
			var op = (OpCode)(instructions[i]);
			if (!Op.Definitions.TryGetValue(op, out var def))
			{
				throw new CompileException($"Can't find OpCode {instructions[i]}.");
			}
			// var operands = instructions.co
			var read = def.ReadOperands(instructions,i+1);
			if ((OpCode)instructions[i] != op)
			{
				Console.WriteLine("dafuqs");
			}
			string instruction = "";
			
			
			switch (def.OperandWidths.Length)
			{
				case 0:
					instruction = $"{def.Name}";
					break;
				case 1:
					instruction = $"{def.Name} {read.Item1[0]}";
					break;
				case 2:
					instruction = $"{def.Name} {read.Item1[0]} {read.Item1[1]}";
					break;
			}
			sb.AppendFormat("{0:0000} {1:0}\n", i, instruction);
			i += read.Item2;
		}

		return sb.ToString();
	}
	public static byte[] ConcatInstructions(byte[][] expectedInstructions)
	{
		var expInstructions = new List<byte>();
		//theres's a way to do this with array.copy or linq concat but i can't be bothered to look it up right now.
		foreach (byte[] bytes in expectedInstructions)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				expInstructions.Add(bytes[i]);
			}
		}

		return expInstructions.ToArray();
	}

	
}