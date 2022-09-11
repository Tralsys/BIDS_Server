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
