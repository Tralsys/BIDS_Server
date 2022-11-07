namespace BIDS.Parser;

public record BIDSBinaryData_State(
	double Location_m,
	float Speed_km,
	float ElectricCurrent_A,
	float LineVoltage_V,
	int Time_ms,
	float BCPressure_kPa,
	float MRPressure_kPa,
	float ERPressure_kPa,
	float BPPressure_kPa,
	float SAPPressure_kPa,
	int NextSignalState,
	byte IsDoorClosed
) : IBIDSBinaryData.IState
{
	const int CONTENT_LENGTH = 49;
	public int ContentLength { get; } = CONTENT_LENGTH;

	public byte RawCommandType { get; } = BinaryParser.INFO_CMD_TYPE_VALUE;

	public const byte RAW_DATA_TYPE = 1;
	public byte RawDataType { get; } = RAW_DATA_TYPE;

	public static IBIDSBinaryData From(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length < CONTENT_LENGTH)
			return new BIDSBinaryData_Error(BIDSBinaryDataErrorType.FewLength);

		return new BIDSBinaryData_State(
			BitConverter.ToDouble(bytes[0..]),
			BitConverter.ToSingle(bytes[8..]),
			BitConverter.ToSingle(bytes[12..]),
			BitConverter.ToSingle(bytes[16..]),
			BitConverter.ToInt32(bytes[20..]),
			BitConverter.ToSingle(bytes[24..]),
			BitConverter.ToSingle(bytes[28..]),
			BitConverter.ToSingle(bytes[32..]),
			BitConverter.ToSingle(bytes[36..]),
			BitConverter.ToSingle(bytes[40..]),
			BitConverter.ToInt32(bytes[44..]),
			bytes[48]
		);
	}

	public bool TryWriteToSpan(Span<byte> bytes)
	{
		if (bytes.Length < CONTENT_LENGTH)
			return false;

		BitConverter.GetBytes(Location_m)
			.CopyTo(bytes[0..]);
		BitConverter.GetBytes(Speed_km)
			.CopyTo(bytes[8..]);
		BitConverter.GetBytes(ElectricCurrent_A)
			.CopyTo(bytes[12..]);
		BitConverter.GetBytes(LineVoltage_V)
			.CopyTo(bytes[16..]);
		BitConverter.GetBytes(Time_ms)
			.CopyTo(bytes[20..]);
		BitConverter.GetBytes(BCPressure_kPa)
			.CopyTo(bytes[24..]);
		BitConverter.GetBytes(MRPressure_kPa)
			.CopyTo(bytes[28..]);
		BitConverter.GetBytes(ERPressure_kPa)
			.CopyTo(bytes[32..]);
		BitConverter.GetBytes(BPPressure_kPa)
			.CopyTo(bytes[36..]);
		BitConverter.GetBytes(SAPPressure_kPa)
			.CopyTo(bytes[40..]);
		BitConverter.GetBytes(NextSignalState)
			.CopyTo(bytes[44..]);

		bytes[48] = IsDoorClosed;

		return true;
	}
}
