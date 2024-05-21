using System.Data;
using System.Text;

namespace scrub_lang.Code;

//this is the first time i've used this enum: type casting syntax and it's also probably the list time I ever will.
public enum OpCode: byte
{
	OpConstant,
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
}

public static class Op
{
	public static Dictionary<OpCode, Definition> Definitions = new Dictionary<OpCode, Definition>()
	{
		{OpCode.OpConstant, new Definition("OpConstant",new int[]{2})}
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
}