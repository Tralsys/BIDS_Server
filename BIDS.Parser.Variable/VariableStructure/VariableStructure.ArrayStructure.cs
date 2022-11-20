using System;
using System.Collections.Generic;
using System.Linq;

namespace BIDS.Parser.Variable;

public partial record VariableStructure
{
	public record ArrayStructure(VariableDataType ElemType, string Name, Array? ValueArray = null) : IDataRecord
	{
		VariableDataType IDataRecord.Type => VariableDataType.Array;

		public IDataRecord With(ref ReadOnlySpan<byte> bytes)
		{
			int arrayLength = Utils.GetInt32AndMove(ref bytes);

			if (ElemType == VariableDataType.UInt8)
			{
				byte[] content = bytes[..arrayLength].ToArray();
				bytes = bytes[arrayLength..];

				return this with
				{
					ValueArray = content
				};
			}

			if (ElemType.GetSpecifiedTypeArray(arrayLength) is not Array array)
				throw new Exception($"Cannot create array with type {ElemType}");

			for (int i = 0; i < arrayLength; i++)
				array.SetValue(this.ElemType.GetValueAndMoveNext(ref bytes), i);

			return this with
			{
				ValueArray = array
			};
		}

		public IEnumerable<byte> GetStructureBytes()
			=> BitConverter.GetBytes((int)VariableDataType.Array)
					.Concat(BitConverter.GetBytes((int)this.ElemType))
					.Concat(System.Text.Encoding.UTF8.GetBytes(this.Name)).Append((byte)0);

		public IEnumerable<byte> GetBytes()
		{
			IEnumerable<byte> arr = BitConverter.GetBytes(this.ValueArray?.Length ?? 0);

			if (this.ValueArray is byte[] byteArray)
				arr = arr.Concat(byteArray);
			else if (this.ValueArray is not null)
			{
				foreach (var v in this.ValueArray)
					arr = arr.Concat(this.ElemType.GetBytes(v));
			}

			return arr;
		}
	}
}
