using System;
using System.Collections.Generic;

using BIDS.Parser.Variable;

using NUnit.Framework;

namespace BIDS.Parser.Variable.Tests;

public class VariableStructureTests
{
	[TestCase(VariableDataType.Boolean, "bool", new byte[] { 0 }, false)]
	[TestCase(VariableDataType.Boolean, "bool", new byte[] { 1 }, true)]

	[TestCase(VariableDataType.Int8, "int8", new byte[] { 2 }, (sbyte)2)]
	[TestCase(VariableDataType.Int8, "int8", new byte[] { 0xFF }, (sbyte)-1)]
	[TestCase(VariableDataType.Int16, "int16", new byte[] { 0x00, 0x01 }, (short)256)]
	[TestCase(VariableDataType.Int16, "int16", new byte[] { 0xFF, 0xFF }, (short)-1)]
	[TestCase(VariableDataType.Int32, "int32", new byte[] { 0x00, 0x00, 0x00, 0x01 }, (int)0x01000000)]
	[TestCase(VariableDataType.Int32, "int32", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, (int)-1)]
	[TestCase(VariableDataType.Int64, "int64", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }, (long)0x0100000000000000)]
	[TestCase(VariableDataType.Int64, "int64", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, (long)-1)]

	[TestCase(VariableDataType.UInt8, "uint8", new byte[] { 2 }, (byte)2)]
	[TestCase(VariableDataType.UInt8, "uint8", new byte[] { 0xFF }, (byte)255)]
	[TestCase(VariableDataType.UInt16, "uint16", new byte[] { 0x00, 0x01 }, (ushort)256)]
	[TestCase(VariableDataType.UInt16, "uint16", new byte[] { 0xFF, 0xFF }, ushort.MaxValue)]
	[TestCase(VariableDataType.UInt32, "uint32", new byte[] { 0x00, 0x00, 0x00, 0x01 }, (uint)0x01000000)]
	[TestCase(VariableDataType.UInt32, "uint32", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, uint.MaxValue)]
	[TestCase(VariableDataType.UInt64, "uint64", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }, (ulong)0x0100000000000000)]
	[TestCase(VariableDataType.UInt64, "uint64", new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, ulong.MaxValue)]

	// Float16 is not supported
	//[TestCase(VariableDataType.Float16, "float16", new byte[] { 0x00, 0x3D }, 1.25)]
	[TestCase(VariableDataType.Float32, "float32", new byte[] { 0x00, 0x00, 0xA0, 0x3F }, 1.25f)]
	[TestCase(VariableDataType.Float64, "float64", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF4, 0x3F }, 1.25)]
	public void SingleRecordTest(VariableDataType type, string name, byte[] bytes, object? expected)
	{
		string structureName = "SampleStrcuture";
		VariableStructure structure = new(0, structureName, new List<VariableStructure.IDataRecord>()
		{
			new VariableStructure.DataRecord(type, name, null)
		});

		VariableStructurePayload actual = structure.With(bytes.AsSpan());

		Assert.That(actual.TryGetValue(name, out var actualRecord), Is.True);
		Assert.That(actualRecord, Is.EqualTo(new VariableStructure.DataRecord(type, name, expected)));
	}

	[Test]
	public void SingleArrayTest()
	{
		VariableDataType type = VariableDataType.Int8;
		string name = "array";
		string structureName = "SampleStrcuture";
		VariableStructure structure = new(0, structureName, new List<VariableStructure.IDataRecord>()
		{
			new VariableStructure.ArrayStructure(type, name, null)
		});

		byte[] bytes = new byte[]
		{
			4,
			0,
			0,
			0,

			1,
			2,
			0xFF,
			0xFE
		};

		sbyte[] expected = new sbyte[]
		{
			1,
			2,
			-1,
			-2
		};

		VariableStructurePayload actual = structure.With(bytes.AsSpan());

		Assert.That(actual.TryGetValue(name, out var actualRecord), Is.True);
		if (actualRecord is not VariableStructure.ArrayStructure actualArrayRecord)
		{
			Assert.Fail("actualRecord was not ArrayStructure");
			return;
		}

		Assert.That(actualArrayRecord.ElemType, Is.EqualTo(type));
		Assert.That(actualArrayRecord.Name, Is.EqualTo(name));
		Assert.That(actualArrayRecord.ValueArray, Is.EquivalentTo(expected));
	}

	[Test]
	public void MultiRecordTest()
	{
		string structureName = "SampleStrcuture";
		VariableStructure structure = new(0, structureName, new List<VariableStructure.IDataRecord>()
		{
			new VariableStructure.DataRecord(VariableDataType.Boolean, "Bool", null),
			new VariableStructure.ArrayStructure(VariableDataType.UInt8, "Array", null),
			new VariableStructure.DataRecord(VariableDataType.Int32, "Int32", null),
		});

		byte[] bytes = new byte[]
		{
			// Boolean = true
			1,

			// Array (Length=4, Elem={1, 2, 255, 254})
			4,
			0,
			0,
			0,

			1,
			2,
			0xFF,
			0xFE,

			// Int32 = -2
			0xFE,
			0xFF,
			0xFF,
			0xFF,
		};

		byte[] expected_array = new byte[]
		{
			1,
			2,
			255,
			254
		};

		VariableStructurePayload actual = structure.With(bytes.AsSpan());

		Assert.That(actual, Has.Count.EqualTo(3));

		Assert.Multiple(() =>
		{
			Assert.That(actual["Bool"], Is.EqualTo(new VariableStructure.DataRecord(VariableDataType.Boolean, "Bool", true)));
			Assert.That(actual["Int32"], Is.EqualTo(new VariableStructure.DataRecord(VariableDataType.Int32, "Int32", -2)));

			if (actual["Array"] is not VariableStructure.ArrayStructure arrayStructure)
				Assert.Fail("dataName='Array' was not VariableStructure.ArrayStructure");
			else
				Assert.That(arrayStructure.ValueArray, Is.EquivalentTo(expected_array));
		});
	}
}
