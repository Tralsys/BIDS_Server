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
	public int ContentLength => Structure.GetStructureBytes().Count();

	public bool TryWriteToSpan(Span<byte> bytes)
	{
		byte[] arr = Structure.GetStructureBytes().ToArray();

		if (bytes.Length < arr.Length)
			return false;

		arr.CopyTo(bytes);

		return true;
	}
}

public record VariablePayload(
	VariableStructurePayload Payload,
	VariableStructure? Structure = null
) : IBIDSBinaryData.IVariablePayload
{
	public byte RawCommandType { get; } = BinaryParser.VARIABLE_CMD_TYPE_VALUE;

	public byte RawDataType { get; } = VariableStructureRegister.RAW_DATA_TYPE;

	// TODO: VariableStructure側を書き直して、効率良くLengthを取得できるようにする
	public int ContentLength => Structure?.GetBytes().Count() ?? 0;

	public bool TryWriteToSpan(Span<byte> bytes)
	{
		if (Structure is null)
			return false;

		byte[] arr = Structure.GetBytes().ToArray();

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

			VariableStructurePayload v => new VariablePayload(v),

			VariableCmdKeyNotFound v => new BIDSBinaryData_Error(BIDSBinaryDataErrorType.UnknownVariableDataKey),

			_ => new BIDSBinaryData_Error(BIDSBinaryDataErrorType.Unknown)
		};
	}
}
