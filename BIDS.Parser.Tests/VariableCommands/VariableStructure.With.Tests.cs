using System;
using System.Collections.Generic;

using BIDS.Parser.VariableCommands;

using NUnit.Framework;

namespace BIDS.Parser.Tests.VariableCommands;

public class VariableStructureTests
{
	[TestCase(VariableDataType.Boolean, "bool", new byte[] { 0 }, false)]
	[TestCase(VariableDataType.Boolean, "bool", new byte[] { 1 }, true)]
	public void SingleRecordTest(VariableDataType type, string name, byte[] bytes, object? expected)
	{
		VariableStructure structure = new(0, new List<VariableStructure.IDataRecord>()
		{
			new VariableStructure.DataRecord(type, name, null)
		});

		VariableStructurePayload actual = structure.With(bytes.AsSpan());

		Assert.That(actual.TryGetValue(name, out var actualRecord), Is.True);
		Assert.That(actualRecord, Is.EqualTo(new VariableStructure.DataRecord(type, name, expected)));
	}
}
