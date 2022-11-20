namespace BIDS.Parser.Variable.Tests.IDataRecordTests;

public class ArrayStructureTests
{
	[Test]
	public void GetStructureBytesTest()
	{
		string name = "abc";

		VariableDataType type = VariableDataType.Int32;

		byte[] expected = new byte[]
		{
			// VariableDataType.Array == 17
			0x11,
			0x00,
			0x00,
			0x00,

			// VariableDataType.Int32 == 3
			3,
			0,
			0,
			0,

			(byte)'a',
			(byte)'b',
			(byte)'c',
			0
		};

		VariableStructure.ArrayStructure data = new(type, name);

		Assert.That(data.GetStructureBytes(), Is.EquivalentTo(expected));
	}

	[TestCase(
		VariableDataType.Int16,
		new short[]
		{
			1,
			2,
			-3,
		},
		new byte[]
		{
			// count (length) = 3
			0x03,
			0x00,
			0x00,
			0x00,

			// content[0] = 1
			0x01,
			0x00,
			// content[1] = 2
			0x02,
			0x00,
			// content[2] = -3
			0xFD,
			0xFF,
		})]
	[TestCase(
		VariableDataType.UInt8,
		new byte[]
		{
			1,
			2,
			0x80,
		},
		new byte[]
		{
			// count (length) = 3
			0x03,
			0x00,
			0x00,
			0x00,

			// content[0] = 1
			0x01,
			// content[1] = 2
			0x02,
			// content[2] = 128
			0x80,
		})]
	public void GetBytesTest(VariableDataType type, Array value, byte[] expected)
	{
		VariableStructure.ArrayStructure data = new(type, "test", value);

		Assert.That(data.GetBytes(), Is.EquivalentTo(expected));
	}
}
