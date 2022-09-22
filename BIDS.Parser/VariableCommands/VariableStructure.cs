namespace BIDS.Parser.VariableCommands;

public record ValiableStructure(int DataTypeId, IReadOnlyList<ValiableStructure.DataRecord> Records) : IValiableCmdResult
{
	public interface IDataRecord
	{
		VariableDataType Type { get; }

		public IDataRecord With(ref ReadOnlySpan<byte> bytes);
	}

	public record DataRecord(VariableDataType Type) : IDataRecord
	{
		public IDataRecord With(ref ReadOnlySpan<byte> bytes)
		{
			return null;
		}
	}

	public record ArrayStructure(VariableDataType ElemType) : IDataRecord
	{
		VariableDataType IDataRecord.Type => VariableDataType.Array;

		public IDataRecord With(ref ReadOnlySpan<byte> bytes)
		{
			return null;
		}
	}

	/// <summary>この構造を用いて、指定のバイト配列を解析する</summary>
	/// <param name="bytes">受け取ったデータ</param>
	/// <returns>解析結果</returns>
	public ValiableStructurePayload With(ReadOnlySpan<byte> bytes)
	{
		return null;
	}
};
