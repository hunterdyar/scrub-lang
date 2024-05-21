using System.Data;
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
	public (int[], int) ReadOperands(byte[] instructions)
	{
		var operands = new int[this.OperandWidths.Length];
		var offset = 0;
		for (int i = 0; i < OperandWidths.Length; i++)
		{
			switch (OperandWidths[i])
			{
				case 0: break;
				case 1:
					operands[i] = instructions[offset];
					break;
				case 2:
					if (BitConverter.IsLittleEndian)
					{
						operands[i] = BitConverter.ToInt16([instructions[offset + 1], instructions[offset]]);
					}
					else
					{
						operands[i] = BitConverter.ToInt16(instructions, offset);
					}
					break;
			}

			offset += OperandWidths[i];
		}

		return (operands, offset);
	}
}

public static class Op
{
	public static Dictionary<OpCode, Definition> Definitions = new Dictionary<OpCode, Definition>()
	{
		{ OpCode.OpConstant, new Definition("OpConstant", new int[] { 2 }) },
		{ OpCode.OpPop, new Definition("OpPop", new int[] { })},
		{ OpCode.OpAdd, new Definition("OpAdd", new int[] { })},
		{ OpCode.OpMult, new Definition("OpAdd", new int[] { })},
		{ OpCode.OpSubtract, new Definition("OpAdd", new int[] { })},
		{ OpCode.OpDivide, new Definition("OpAdd", new int[] { }) },
		{ OpCode.OpTrue, new Definition("OpAdd", new int[] { })},
		{ OpCode.OpFalse, new Definition("OpAdd", new int[] { })},
		{ OpCode.OpEqual, new Definition("OpAdd", new int[] { }) },
		{ OpCode.OpNotEqual, new Definition("OpAdd", new int[] { }) },
		{ OpCode.OpGreaterThan, new Definition("OpAdd", new int[] { }) },
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

	//This is the opposite of Make.
	
	//helper
	
	//minidissembler
	public static string InstructionsToString(byte[] instructions)
	{
		var sb = new StringBuilder();
		for (int i = 0; i < instructions.Length; i++)
		{
			if (!Op.Definitions.TryGetValue((OpCode)instructions[i], out var def))
			{
				throw new CompileException($"Can't find OpCode {instructions[i]}.");
			}

			var read = def.ReadOperands(instructions);
			string instruction = "";
			
			//todo: THis is broken.
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