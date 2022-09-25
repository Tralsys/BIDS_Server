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

		int dataId = Utils.GetInt32AndMove(ref gotData);

		return dataId switch
		{
			// DataType Register
			0 => ParseDataTypeRegisterCommand(gotData),

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

		int cmdDataType = Utils.GetInt32AndMove(ref bytes);

		while (bytes.Length > 5)
		{
			// 各フィールドのデータ型番号を取得する
			VariableDataType dataType = (VariableDataType)Utils.GetInt32AndMove(ref bytes);

			// 配列の場合は、配列の型に関する情報が次に記録されている。
			VariableDataType? arrayElemDataType = dataType == VariableDataType.Array
				? arrayElemDataType = (VariableDataType)Utils.GetInt32AndMove(ref bytes)
				: null;

			// 変数名(フィールド名/データ名)を取得する。
			// NULL文字に到達するか、あるいはSpanを全て読み切ったら終了
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
