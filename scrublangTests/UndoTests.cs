using scrub_lang.Objects;
using scrub_lang.VirtualMachine;

namespace scrublangTests;

public class UndoTests
{
	[Test]
	public void TestAssignStatementsUndo()
	{
		new VMTestCase("one = 1", new Integer(1), TestType.ZigZagWithUndo);
		new VMTestCase("one = 1\n one", new Integer(1));
		new VMTestCase("one = 1\n two = 2\n one + two", new Integer(3), TestType.ZigZagWithUndo);
		new VMTestCase("one = 1\ntwo = one+one\n one+two", new Integer(3), TestType.ZigZagWithUndo);
	}

	[Test]
	public void TestFunctionCallsUndo()
	{
		new VMTestCase("func add(a,b){a+b};add(1,2)", new Integer(3),TestType.ZigZagWithUndo);
		new VMTestCase("func sub(a,b){a-b};sub(1,2)", new Integer(-1),TestType.ZigZagWithUndo);
	}
}