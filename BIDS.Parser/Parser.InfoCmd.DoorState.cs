using BIDS.Parser.Internals;

namespace BIDS.Parser;

public record BIDSCmd_Info_DoorState(
	char RawDataType,
	int RawDataNum,
	DoorStateType DoorStateType,
	IReadOnlyList<int> DataInt,
	IReadOnlyList<double> DataDouble
) : BIDSCmd_Info(RawDataType, RawDataNum, DataInt, DataDouble), IBIDSCmd_Info_DoorState
{
	public BIDSCmd_Info_DoorState(BIDSCmd_Info Base, DoorStateType type)
		: this(Base.RawDataType, Base.RawDataNum, type, Base.DataInt, Base.DataDouble) { }
}

public partial class Parser
{
	static DoorStateType GetDoorStateType(int dataTypeInt)
		=> dataTypeInt switch
		{
			0 => DoorStateType.IsOpen,
			_ => DoorStateType.Unknown,
		};
}
