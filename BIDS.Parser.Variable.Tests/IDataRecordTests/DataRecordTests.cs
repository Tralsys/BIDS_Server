namespace BIDS.Parser.Variable.Tests.IDataRecordTests;

public class DataRecordTests
{
	[Test]
	public void GetStructureBytesTest()
	{
		string name = "abc";

		// VariableDataType.Int32 == 3
		VariableDataType type = VariableDataType.Int32;

		byte[] expected = new byte[]
		{
			3,
			0,
			0,
			0,

			(byte)'a',
			(byte)'b',
			(byte)'c',
			0
		};

		VariableStructure.DataRecord data = new(type, name);

		Assert.That(data.GetStructureBytes(), Is.EquivalentTo(expected));
	}

	[TestCase(VariableDataType.Int16, (short)-3, new byte[] { 0xFD, 0xFF })]
	public void GetBytesTest(VariableDataType type, object value, byte[] expected)
	{
		VariableStructure.DataRecord data = new(type, "test", value);

		Assert.That(data.GetBytes(), Is.EquivalentTo(expected));
	}
}
