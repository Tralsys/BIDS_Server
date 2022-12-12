using System.Linq;
using BIDS.Parser.Variable;

namespace BIDS.Parser;

public record VariableStructureRegister(
	VariableStructure Structure
) : IBIDSBinaryData.IVariableStructureRegister
{
	public const byte RAW_COMMAND_TYPE = (byte)'v';
	public byte RawCommandType { get; } = BinaryParser.VARIABLE_CMD_TYPE_VALUE;

	public const byte RAW_DATA_TYPE = 0;
	public byte RawDataType { get; } = RAW_DATA_TYPE;

	// TODO: VariableStructure側を書き直して、効率良くLengthを取得できるようにする
	public int ContentLength => this.GetBytes().Count();

	IEnumerable<byte> GetBytes()
		=> new byte[sizeof(int)].Concat(Structure.GetStructureBytes());

	public bool TryWriteToSpan(Span<byte> bytes)
	{
		byte[] arr = this.GetBytes().ToArray();

		if (bytes.Length < arr.Length)
			return false;

		arr.CopyTo(bytes);

		return true;
	}
}

public record VariablePayload(
	VariableStructurePayload Payload,
	VariableStructure Structure
) : IBIDSBinaryData.IVariablePayload
{
	public byte RawCommandType { get; } = BinaryParser.VARIABLE_CMD_TYPE_VALUE;

	public byte RawDataType { get; } = VariableStructureRegister.RAW_DATA_TYPE;

	// TODO: VariableStructure側を書き直して、効率良くLengthを取得できるようにする
	public int ContentLength => GetContentBytes().Length;

	byte[] GetContentBytes()
	{
		IEnumerable<byte> arr = BitConverter.GetBytes(Structure.DataTypeId);

		foreach (var v in Structure.Records)
		{
			if (Payload.TryGetValue(v.Name, out VariableStructure.IDataRecord value))
			{
				// TODO: 型の不一致チェックをしないと、メモリがぶっ壊れるかも
				arr = arr.Concat(value.GetBytes());
			}
			else
			{
				arr = arr.Concat(v.GetBytes());
			}
		}

		return arr.ToArray();
	}

	public bool TryWriteToSpan(Span<byte> bytes)
	{
		byte[] arr = GetContentBytes();

		if (bytes.Length < arr.Length)
			return false;

		arr.CopyTo(bytes);

		return true;
	}
}

public partial class BinaryParser
{
	IBIDSBinaryData OnVariableCmdGot(ReadOnlySpan<byte> contents)
	{
		// 可変長データ
		IVariableCmdResult result = VariableCmdParser.From(contents);

		return result switch
		{
			VariableStructure v => new VariableStructureRegister(v),

			VariableStructurePayload v => new VariablePayload(v, VariableCmdParser.DataTypeDict[v.DataTypeId]),

			VariableCmdKeyNotFound v => new BIDSBinaryData_Error(BIDSBinaryDataErrorType.UnknownVariableDataKey),

			_ => new BIDSBinaryData_Error(BIDSBinaryDataErrorType.Unknown)
		};
	}
}
