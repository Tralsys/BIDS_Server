namespace BIDS.Parser.VariableCommands;

public record VariableStructure(int DataTypeId, IReadOnlyList<VariableStructure.IDataRecord> Records) : IVariableCmdResult
{
	public interface IDataRecord
	{
		VariableDataType Type { get; }

		string Name { get; }

		IDataRecord With(ref ReadOnlySpan<byte> bytes);
	}

	public record DataRecord(VariableDataType Type, string Name, object? Value = null) : IDataRecord
	{
		public IDataRecord With(ref ReadOnlySpan<byte> bytes)
		{
			return this with
			{
				Value = this.Type.GetValueAndMoveNext(ref bytes)
			};
		}
	}

	public record ArrayStructure(VariableDataType ElemType, string Name, object?[]? ValueArray = null) : IDataRecord
	{
		VariableDataType IDataRecord.Type => VariableDataType.Array;

		public IDataRecord With(ref ReadOnlySpan<byte> bytes)
		{
			int arrayLength = Utils.GetInt32AndMove(ref bytes);

			object?[] array = new object?[arrayLength];
			for (int i = 0; i < arrayLength; i++)
				array[i] = this.ElemType.GetValueAndMoveNext(ref bytes);

			return this with
			{
				ValueArray = array
			};
		}
	}

	/// <summary>この構造を用いて、指定のバイト配列を解析する</summary>
	/// <param name="bytes">受け取ったデータ</param>
	/// <returns>解析結果</returns>
	public VariableStructurePayload With(ReadOnlySpan<byte> bytes)
	{
		VariableStructurePayload payload = new(this.DataTypeId);

		foreach (var recordInfo in this.Records)
		{
			IDataRecord data = recordInfo.With(ref bytes);

			payload.Add(recordInfo.Name, data);
		}

		return payload;
	}
};
