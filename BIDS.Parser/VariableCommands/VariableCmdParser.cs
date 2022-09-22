namespace BIDS.Parser.VariableCommands;

public interface IValiableCmdResult
{
	int DataTypeId { get; }
}

public class ValiableStructurePayload : Dictionary<string, ValiableStructure.IDataRecord>, IValiableCmdResult
{
	public int DataTypeId { get; }

	public ValiableStructurePayload(int dataTypeId)
	{
		DataTypeId = dataTypeId;
	}
}

public record ValiableCmdKeyNotFound(int DataTypeId) : IValiableCmdResult;

public record ValiableCmdError();

public class VariableCmdParser
{
	Dictionary<int, ValiableStructure> DataTypeDict { get; } = new();

	public IValiableCmdResult From(ReadOnlySpan<byte> gotData)
	{
		if (gotData.Length < 4)
			throw new InvalidDataException("Not a Valiable Command (length < 4)");

		int dataId = BitConverter.ToInt32(gotData[0..4]);

		return dataId switch
		{
			// DataType Register
			0 => ParseDataTypeRegisterCommand(gotData[4..]),

			_ => DataTypeDict.TryGetValue(dataId, out ValiableStructure? structure)
				? structure.With(gotData)
				: new ValiableCmdKeyNotFound(dataId)
		};
	}

	private ValiableStructure ParseDataTypeRegisterCommand(ReadOnlySpan<byte> bytes)
	{
		// 前から順々にデータ構造を記録
		List<ValiableStructure.IDataRecord> records = new();

		// 初期段階では、カスタムデータを構造に含めることはサポートしない

		return null;
	}
}
