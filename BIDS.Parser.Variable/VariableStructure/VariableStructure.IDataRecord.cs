using System;
using System.Collections.Generic;

namespace BIDS.Parser.Variable;

public partial record VariableStructure
{
	public interface IDataRecord
	{
		VariableDataType Type { get; }

		string Name { get; }

		IDataRecord With(ref ReadOnlySpan<byte> bytes);

		IEnumerable<byte> GetStructureBytes();

		IEnumerable<byte> GetBytes();
	}
}
