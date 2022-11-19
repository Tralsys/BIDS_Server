using System;
using System.Collections.Generic;
using System.Linq;

namespace BIDS.Parser.Variable;

public partial record VariableStructure
{
	public record DataRecord(VariableDataType Type, string Name, object? Value = null) : IDataRecord
	{
		public IDataRecord With(ref ReadOnlySpan<byte> bytes)
		{
			return this with
			{
				Value = this.Type.GetValueAndMoveNext(ref bytes)
			};
		}

		public IEnumerable<byte> GetStructureBytes()
			=> BitConverter.GetBytes((int)this.Type).Concat(System.Text.Encoding.UTF8.GetBytes(this.Name)).Append((byte)0);

		public IEnumerable<byte> GetBytes()
			=> this.Type.GetBytes(this.Value);
	}
}
