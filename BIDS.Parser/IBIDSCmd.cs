namespace BIDS.Parser;

public interface IBIDSCmd
{
	string ToCommandStr();
}

public interface IBIDSCmd_ReverserControl : IBIDSCmd
{
	ReverserPos ReverserPos { get; }
	int? Value { get; }
}

public interface IBIDSCmd_StandardStyle : IBIDSCmd
{
	int Value { get; }
}

public interface IBIDSCmd_KeyControl : IBIDSCmd
{
	KeyType KeyType { get; }
	KeyControlType ControlType { get; }
	int KeyNumber { get; }
}

public interface IBIDSCmd_Info : IBIDSCmd
{
	char RawDataType { get; }

	int RawDataNum { get; }

	IReadOnlyList<int> DataInt { get; }

	IReadOnlyList<double> DataDouble { get; }
}

public interface IBIDSCmd_Info_SpecData : IBIDSCmd_Info
{
	SpecDataType SpecDataType { get; }
}

public interface IBIDSCmd_Info_StateData : IBIDSCmd_Info
{
	StateDataType StateDataType { get; }
}

public interface IBIDSCmd_Info_DoorState : IBIDSCmd_Info
{
	DoorStateType DoorStateType { get; }
}

public interface IBIDSCmd_Info_HandlePosition : IBIDSCmd_Info
{
	HandlePosType HandlePosType { get; }
}

public interface IBIDSCmd_Info_ArrayData : IBIDSCmd_Info
{
	IReadOnlyList<int> IndexList { get; }
}

public interface IBIDSCmd_Info_PanelData : IBIDSCmd_Info_ArrayData { }
public interface IBIDSCmd_Info_SoundData : IBIDSCmd_Info_ArrayData { }
