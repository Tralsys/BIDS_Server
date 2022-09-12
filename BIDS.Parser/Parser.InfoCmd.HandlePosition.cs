using BIDS.Parser.Internals;

namespace BIDS.Parser;

public record BIDSCmd_Info_HandlePosition(
	char RawDataType,
	int RawDataNum,
	HandlePosType HandlePosType,
	IReadOnlyList<int>? DataInt,
	IReadOnlyList<double>? DataDouble
) : BIDSCmd_Info(RawDataType, RawDataNum, DataInt, DataDouble), IBIDSCmd_Info_HandlePosition
{
	public BIDSCmd_Info_HandlePosition(BIDSCmd_Info Base, HandlePosType type)
		: this(Base.RawDataType, Base.RawDataNum, type, Base.DataInt, Base.DataDouble) { }
}

public partial class Parser
{
	static HandlePosType GetHandlePosType(int dataTypeInt)
		=> dataTypeInt switch
		{
			0 => HandlePosType.Brake,
			1 => HandlePosType.Power,
			2 => HandlePosType.Reverser,
			3 => HandlePosType.ConstSpeed,
			_ => HandlePosType.Unknown,
		};
}
