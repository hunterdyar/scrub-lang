using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using scrub_lang.Compiler;

namespace scrub_lang.Compiler;

//this is the first time i've used this enum: type casting syntax and it's also probably the list time I ever will.
public enum OpCode: byte
{
	OpConstant,
	OpAdd,
	OpPop,
	OpSubtract,
	OpMult,
	OpDivide,
	OpTrue,
	OpFalse,
	OpEqual,
	OpNotEqual,
	OpGreaterThan,
	OpBang,
	OpNegate,
	OpJump,
	OpJumpNotTruthy,
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

	//Todo: I am working on fixing this broken function.
	//this is the opposite of Make()
	public (UInt16[], int) ReadOperands(byte[] instructions, int start)
	{
		var operands = new UInt16[this.OperandWidths.Length];
		int offset = start;//start of operands, not of operation.
		//x operands, each y units widw. For each operand...
		for (int i = 0; i < OperandWidths.Length; i++)
		{
			switch (OperandWidths[i])// this operand is this many bytes.
			{
				case 0: break;
				case 1:
					operands[i] = instructions[offset];
					break;
				case 2:
					operands[i] = Op.ReadUInt16([instructions[offset+0], instructions[offset +1]]);
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
	public static Dictionary<OpCode, Definition> Definitions = new Dictionary<OpCode, Definition>()
	{
		{ OpCode.OpConstant, new Definition("OpConstant", new int[] { 2 }) },
		{ OpCode.OpPop, new Definition("OpPop", new int[] { })},
		{ OpCode.OpAdd, new Definition("OpAdd", new int[] { })},
		{ OpCode.OpMult, new Definition("OpMult", new int[] { })},
		{ OpCode.OpSubtract, new Definition("OpSubtract", new int[] { })},
		{ OpCode.OpDivide, new Definition("OpDivide", new int[] { }) },
		{ OpCode.OpTrue, new Definition("OpTrue", new int[] { })},
		{ OpCode.OpFalse, new Definition("OpFalse", new int[] { })},
		{ OpCode.OpEqual, new Definition("OpEqual", new int[] { }) },
		{ OpCode.OpNotEqual, new Definition("OpNotEqual", new int[] { }) },
		{ OpCode.OpGreaterThan, new Definition("OpGreaterThan", new int[] { }) },
		{ OpCode.OpNegate, new Definition("OpNegate", new int[] { }) },
		{ OpCode.OpBang, new Definition("OpBang", new int[] { }) },
		{ OpCode.OpJump, new Definition("OpJump", new int[] { 2 }) },
		{ OpCode.OpJumpNotTruthy, new Definition("OpJumpNotTruthy", new int[] { 2 }) },

	};

	public static byte[] Make(OpCode op, params int[] operands)
	{
		if (!Definitions.TryGetValue(op, out var def))
		{
			return new byte[]{};
		}

		int instructionLength = 1;
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
					var o = BitConverter.GetBytes((char)operands[i]);
					instruction[offset] = o[0];
					break;
				case 2:
					o = BitConverter.GetBytes(((ushort)operands[i]));
					//c# doesn't define endian-ness, so we have to check this flag.
					//note: MiscUtil EndianBitConverter.
					instruction[offset] = BitConverter.IsLittleEndian ? o[1] : o[0];
					instruction[offset + 1] = BitConverter.IsLittleEndian ? o[0] : o[1];
					break;
			}

			offset += width;
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