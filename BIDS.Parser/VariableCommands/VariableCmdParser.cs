using System.Text;

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

	internal static VariableStructure ParseDataTypeRegisterCommand(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length < 9)
			throw new ArgumentException("`bytes` length must be more than 9 (dataType{4} _ elemDataType{4} _ elemName{1..})");

		// 前から順々にデータ構造を記録
		List<VariableStructure.IDataRecord> records = new();

		// 初期段階では、カスタムデータを構造に含めることはサポートしない

		int cmdDataType = BitConverter.ToInt32(bytes[..4]);
		bytes = bytes[4..];

		while (bytes.Length > 5)
		{
			VariableDataType dataType = (VariableDataType)BitConverter.ToInt32(bytes[..4]);
			bytes = bytes[4..];

			VariableDataType? arrayElemDataType = null;
			if (dataType == VariableDataType.Array)
			{
				arrayElemDataType = (VariableDataType)BitConverter.ToInt32(bytes[0..4]);
				bytes = bytes[4..];
			}

			int dataNameLen = 0;
			while (dataNameLen < bytes.Length && bytes[dataNameLen] != 0)
				dataNameLen++;

			string dataName = Encoding.UTF8.GetString(bytes[..dataNameLen]);

			if (arrayElemDataType is VariableDataType elemDataType)
				records.Add(new VariableStructure.ArrayStructure(elemDataType, dataName));
			else
				records.Add(new VariableStructure.DataRecord(dataType, dataName));

			// NULL文字分進めてチェックする
			if (bytes.Length <= (dataNameLen + 1))
				break;
			bytes = bytes[(dataNameLen + 1)..];
		}

		return new VariableStructure(cmdDataType, records);
	}
}
