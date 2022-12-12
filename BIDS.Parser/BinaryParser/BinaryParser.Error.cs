namespace BIDS.Parser;

public enum BIDSBinaryDataErrorType
{
	Unknown,
	FewLength,
	UnknownCommandType,
	UnknownDataType,

	NotBIDSCommand,

	UnknownVariableDataKey,
	UnknownVariableDataName,
}

public record BIDSBinaryData_Error(
	BIDSBinaryDataErrorType ErrorType
) : IBIDSBinaryData
{
	const int CONTENT_LENGTH = 4;
	public int ContentLength { get; } = CONTENT_LENGTH;

	public const byte RAW_COMMAND_TYPE = (byte)'e';
	public byte RawCommandType { get; } = RAW_COMMAND_TYPE;

	public const byte RAW_DATA_TYPE = 0;
	public byte RawDataType { get; } = RAW_DATA_TYPE;

	public static IBIDSBinaryData From(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length < CONTENT_LENGTH)
			return new BIDSBinaryData_Error(BIDSBinaryDataErrorType.FewLength);

		return new BIDSBinaryData_Error(
			(BIDSBinaryDataErrorType)BitConverter.ToInt32(bytes[0..])
		);
	}

	public bool TryWriteToSpan(Span<byte> bytes)
	{
		if (bytes.Length < CONTENT_LENGTH)
			return false;

		BitConverter.GetBytes((int)ErrorType)
			.CopyTo(bytes[0..]);

		return true;
	}
}
