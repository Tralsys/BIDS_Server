using BIDS.Parser.Variable;

namespace BIDS.Parser;

public interface IBIDSBinaryData : IBIDSCmd
{
	const int HEADER_LENGTH = 4;

	// `tR`
	const short RAW_DATA_IDENTIFIER = (short)0x5274;

	byte RawCommandType { get; }
	byte RawDataType { get; }

	int ContentLength { get; }

	bool TryWriteToSpan(Span<byte> bytes);

	byte[] GetBytes()
	{
		byte[] bytes = new byte[ContentLength];

		TryWriteToSpan(bytes);

		return bytes;
	}

	byte[] GetBytesWithHeader()
	{
		byte[] bytes = new byte[HEADER_LENGTH + ContentLength];

		TryWriteToSpanWithHeader(bytes);

		return bytes;
	}

	bool TryWriteToSpanWithHeader(Span<byte> bytes)
	{
		int requredLength = HEADER_LENGTH + ContentLength;
		if (bytes.Length < requredLength)
			return false;

		return
			BitConverter.TryWriteBytes(bytes[0..], RAW_DATA_IDENTIFIER)
			&& BitConverter.TryWriteBytes(bytes[2..], RawCommandType)
			&& BitConverter.TryWriteBytes(bytes[3..], RawDataType)
			&& TryWriteToSpan(bytes[4..]);
	}

	public interface ISpec : IBIDSBinaryData
	{
		int Version { get; }
		short Brake { get; }
		short Power { get; }
		short ATSCheckPos { get; }
		short B67Pos { get; }
		short CarCount { get; }
		short SelfBrake { get; }
	}

	public interface IState : IBIDSBinaryData
	{
		double Location_m { get; }
		float Speed_kmph { get; }
		float ElectricCurrent_A { get; }
		float LineVoltage_V { get; }
		int Time_ms { get; }
		float BCPressure_kPa { get; }
		float MRPressure_kPa { get; }
		float ERPressure_kPa { get; }
		float BPPressure_kPa { get; }
		float SAPPressure_kPa { get; }
		int NextSignalState { get; }
		byte IsDoorClosed { get; }
	}

	public interface IHandle : IBIDSBinaryData
	{
		int Power { get; }
		int Brake { get; }
		int Reverser { get; }
		int SelfBrake { get; }
	}

	public interface IArrayData : IBIDSBinaryData
	{
		byte BiasNum { get; }
		int[] DataArray { get; }
	}

	public interface IVariableStructureRegister : IBIDSBinaryData
	{
		VariableStructure Structure { get; }
	}

	public interface IVariablePayload : IBIDSBinaryData
	{
		VariableStructure Structure { get; }
		VariableStructurePayload Payload { get; }
	}
}
