namespace BIDS.Parser;

public record BIDSBinaryData_Spec(
	int Version,
	short Brake,
	short Power,
	short ATSCheckPos,
	short B67Pos,
	short CarCount,
	short SelfBrake
) : IBIDSBinaryData.ISpec
{
	const int CONTENT_LENGTH = 16;

	public int ContentLength { get; } = CONTENT_LENGTH;

	public byte RawCommandType { get; } = BinaryParser.INFO_CMD_TYPE_VALUE;

	public const byte RAW_DATA_TYPE = 0;
	public byte RawDataType { get; } = RAW_DATA_TYPE;

	public static IBIDSBinaryData From(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length < CONTENT_LENGTH)
			return new BIDSBinaryData_Error(BIDSBinaryDataErrorType.FewLength);

		return new BIDSBinaryData_Spec(
			BitConverter.ToInt32(bytes[0..]),
			BitConverter.ToInt16(bytes[4..]),
			BitConverter.ToInt16(bytes[6..]),
			BitConverter.ToInt16(bytes[8..]),
			BitConverter.ToInt16(bytes[10..]),
			BitConverter.ToInt16(bytes[12..]),
			BitConverter.ToInt16(bytes[14..])
		);
	}

	public bool TryWriteToSpan(Span<byte> bytes)
	{
		if (bytes.Length < CONTENT_LENGTH)
			return false;

		BitConverter.GetBytes(Version)
			.CopyTo(bytes[0..]);
		BitConverter.GetBytes(Brake)
			.CopyTo(bytes[4..]);
		BitConverter.GetBytes(Power)
			.CopyTo(bytes[6..]);
		BitConverter.GetBytes(ATSCheckPos)
			.CopyTo(bytes[8..]);
		BitConverter.GetBytes(B67Pos)
			.CopyTo(bytes[10..]);
		BitConverter.GetBytes(CarCount)
			.CopyTo(bytes[12..]);
		BitConverter.GetBytes(SelfBrake)
			.CopyTo(bytes[14..]);

		return true;
	}
}
