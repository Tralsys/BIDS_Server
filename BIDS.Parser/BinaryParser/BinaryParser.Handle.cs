namespace BIDS.Parser;

public record BIDSBinaryData_Handle(
	int Power,
	int Brake,
	int Reverser,
	int SelfBrake
) : IBIDSBinaryData.IHandle
{
	const int CONTENT_LENGTH = 16;
	public int ContentLength { get; } = CONTENT_LENGTH;

	public byte RawCommandType { get; } = BinaryParser.INFO_CMD_TYPE_VALUE;

	public const byte RAW_DATA_TYPE = 4;
	public byte RawDataType { get; } = RAW_DATA_TYPE;

	public static IBIDSBinaryData From(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length < CONTENT_LENGTH)
			return new BIDSBinaryData_Error(BIDSBinaryDataErrorType.FewLength);

		return new BIDSBinaryData_Handle(
			BitConverter.ToInt32(bytes[0..]),
			BitConverter.ToInt32(bytes[4..]),
			BitConverter.ToInt32(bytes[8..]),
			BitConverter.ToInt32(bytes[12..])
		);
	}

	public bool TryWriteToSpan(Span<byte> bytes)
	{
		if (bytes.Length < CONTENT_LENGTH)
			return false;

		BitConverter.GetBytes(Power)
			.CopyTo(bytes[0..]);
		BitConverter.GetBytes(Brake)
			.CopyTo(bytes[4..]);
		BitConverter.GetBytes(Reverser)
			.CopyTo(bytes[8..]);
		BitConverter.GetBytes(SelfBrake)
			.CopyTo(bytes[12..]);

		return true;
	}
}
