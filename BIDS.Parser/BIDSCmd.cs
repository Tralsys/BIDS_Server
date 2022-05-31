namespace BIDS.Parser;

public interface IBIDSCmd
{
	Identifier Identifier { get; }

	DataType DataType { get; }

	ReverserPos ReverserPos { get; }

	KeyType KeyType { get; }

	SpecDataType SpecDataType { get; }

	StateDataType StateDataType { get; }

	HandlePosType HandlePosType { get; }

	DoorStateType DoorStateType { get; }

	char RawIdentifier { get; }

	char RawDataType { get; }

	int RequestNumber { get; }

	IReadOnlyCollection<int>? DataInt { get; }

	IReadOnlyCollection<double>? DataDouble { get; }

	ParseError? ParseError { get; }
}

public record BIDSCmd(
	Identifier Identifier = Identifier.Unknown,
	DataType DataType = DataType.Unknown,
	ReverserPos ReverserPos = ReverserPos.Unknown,
	KeyType KeyType = KeyType.Unknown,
	SpecDataType SpecDataType = SpecDataType.Unknown,
	StateDataType StateDataType = StateDataType.Unknown,
	HandlePosType HandlePosType = HandlePosType.Unknown,
	DoorStateType DoorStateType = DoorStateType.IsOpen,
	char RawIdentifier = '\0',
	char RawDataType = '\0',
	int RequestNumber = 0,
	IReadOnlyCollection<int>? DataInt = null,
	IReadOnlyCollection<double>? DataDouble = null,
	ParseError? ParseError = null
) : IBIDSCmd;
