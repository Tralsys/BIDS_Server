namespace BIDS.Parser.VariableCommands;

public interface IValiableCmdResult
{
	int DataTypeId { get; }
}

public class VariableStructurePayload : Dictionary<string, VariableStructure.IDataRecord>, IValiableCmdResult
{
	public int DataTypeId { get; }

	public VariableStructurePayload(int dataTypeId)
	{
		DataTypeId = dataTypeId;
	}
}

public record VariableCmdKeyNotFound(int DataTypeId) : IValiableCmdResult;

public record ValiableCmdError();

public class VariableCmdParser
{
	Dictionary<int, VariableStructure> DataTypeDict { get; } = new();

	public IValiableCmdResult From(ReadOnlySpan<byte> gotData)
	{
		if (gotData.Length < 4)
			throw new InvalidDataException("Not a Valiable Command (length < 4)");

		int dataId = BitConverter.ToInt32(gotData[0..4]);

		return dataId switch
		{
			// DataType Register
			0 => ParseDataTypeRegisterCommand(gotData[4..]),

			_ => DataTypeDict.TryGetValue(dataId, out VariableStructure? structure)
				? structure.With(gotData)
				: new VariableCmdKeyNotFound(dataId)
		};
	}

	private VariableStructure ParseDataTypeRegisterCommand(ReadOnlySpan<byte> bytes)
	{
		// 前から順々にデータ構造を記録
		List<VariableStructure.IDataRecord> records = new();

		// 初期段階では、カスタムデータを構造に含めることはサポートしない

		return null;
	}
}
