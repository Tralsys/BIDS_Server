using System;

using BIDS.Parser.VariableCommands;

using NUnit.Framework;

namespace BIDS.Parser.Tests.VariableCommands;

public class ParseDataTypeRegisterCommandTests
{
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(8)]
	public void ArgumentErrorTests(int len)
	{
		byte[] arr = new byte[16];

		Assert.Throws<ArgumentException>(() => VariableCmdParser.ParseDataTypeRegisterCommand(arr.AsSpan()[..len]));
	}

	[TestCase(0, new byte[] {
		(byte)'b',
		(byte)'o',
		(byte)'o',
		(byte)'l',
	}, VariableDataType.Boolean, "bool")]

	[TestCase(1, new byte[] {
		(byte)'i',
		(byte)'n',
		(byte)'t',
		(byte)'8',
	}, VariableDataType.Int8, "int8")]
	[TestCase(2, new byte[] {
		(byte)'i',
		(byte)'n',
		(byte)'t',
		(byte)'1',
		(byte)'6',
	}, VariableDataType.Int16, "int16")]
	[TestCase(3, new byte[] {
		(byte)'i',
		(byte)'n',
		(byte)'t',
		(byte)'3',
		(byte)'2',
	}, VariableDataType.Int32, "int32")]
	[TestCase(4, new byte[] {
		(byte)'i',
		(byte)'n',
		(byte)'t',
		(byte)'6',
		(byte)'4',
	}, VariableDataType.Int64, "int64")]

	[TestCase(6, new byte[] {
		(byte)'u',
		(byte)'i',
		(byte)'n',
		(byte)'t',
		(byte)'8',
	}, VariableDataType.UInt8, "uint8")]
	[TestCase(7, new byte[] {
		(byte)'u',
		(byte)'i',
		(byte)'n',
		(byte)'t',
		(byte)'1',
		(byte)'6',
	}, VariableDataType.UInt16, "uint16")]
	[TestCase(8, new byte[] {
		(byte)'u',
		(byte)'i',
		(byte)'n',
		(byte)'t',
		(byte)'3',
		(byte)'2',
	}, VariableDataType.UInt32, "uint32")]
	[TestCase(9, new byte[] {
		(byte)'u',
		(byte)'i',
		(byte)'n',
		(byte)'t',
		(byte)'6',
		(byte)'4',
	}, VariableDataType.UInt64, "uint64")]

	[TestCase(12, new byte[] {
		(byte)'f',
		(byte)'l',
		(byte)'o',
		(byte)'r',
		(byte)'t',
		(byte)'1',
		(byte)'6',
	}, VariableDataType.Float16, "float16")]
	[TestCase(13, new byte[] {
		(byte)'f',
		(byte)'l',
		(byte)'o',
		(byte)'r',
		(byte)'t',
		(byte)'3',
		(byte)'2',
	}, VariableDataType.Float32, "float32")]
	[TestCase(14, new byte[] {
		(byte)'f',
		(byte)'l',
		(byte)'o',
		(byte)'r',
		(byte)'t',
		(byte)'6',
		(byte)'4',
	}, VariableDataType.Float64, "float64")]
	public void SingleFieldTest(int type, byte[] name, VariableDataType expectedDataType, string expectedName)
	{
		byte[] arr = new byte[8 + name.Length];

		int i = 0;
		foreach (var v in BitConverter.GetBytes(0))
			arr[i++] = v;
		foreach (var v in BitConverter.GetBytes(type))
			arr[i++] = v;
		foreach (var v in name)
			arr[i++] = v;

		var actual = VariableCmdParser.ParseDataTypeRegisterCommand(arr.AsSpan());

		Assert.That(actual.Records.Count, Is.EqualTo(1));

		Assert.That(actual.Records[0], Is.EqualTo(
			new VariableStructure.DataRecord(expectedDataType, expectedName, null))
		);
	}

	[Test]
	public void EmptyFieldNameTest()
	{
		byte[] arr =
		{
			0,
			0,
			0,
			0,

			0,
			0,
			0,
			0,

			0,
		};

		var actual = VariableCmdParser.ParseDataTypeRegisterCommand(arr.AsSpan());

		Assert.That(actual.Records.Count, Is.EqualTo(1));

		Assert.That(actual.Records[0], Is.EqualTo(
			new VariableStructure.DataRecord(VariableDataType.Boolean, "", null))
		);
	}
}
