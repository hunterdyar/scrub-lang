using scrub_lang.Objects;
using scrub_lang.VirtualMachine;

namespace scrublangTests;
using VMTestCase = VMTests.VMTestCase;

public class VariableTests
{
	[Test]
	public void HexLiteralTests()
	{
		new VMTestCase("0xFF", new Integer(255));
		new VMTestCase("0x64", new Integer(100));
		new VMTestCase("0x64 + 0x10", new Integer(116));
		new VMTestCase("0x7fffffff", new Integer(2147483647));//7 is half, it's a signed integer.
	}

	[Test]
	public void BinaryLiteralTests()
	{
		// new VMTestCase("0b", new Integer(0));//todo: throw a more graceful paraser error.
		new VMTestCase("0b0", new Integer(0));
		new VMTestCase("0b11", new Integer(3));
		new VMTestCase("0b01 + 0b11", new Integer(4));
		new VMTestCase("0b11111111", new Integer(255)); //7 is half, it's a signed integer.
	}
}