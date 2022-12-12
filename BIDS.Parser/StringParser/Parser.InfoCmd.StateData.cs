using BIDS.Parser.Internals;

namespace BIDS.Parser;

public record BIDSCmd_Info_StateData(
	char RawDataType,
	int RawDataNum,
	StateDataType StateDataType,
	IReadOnlyList<int>? DataInt,
	IReadOnlyList<double>? DataDouble
) : BIDSCmd_Info(RawDataType, RawDataNum, DataInt, DataDouble), IBIDSCmd_Info_StateData
{
	public BIDSCmd_Info_StateData(BIDSCmd_Info Base, StateDataType type)
		: this(Base.RawDataType, Base.RawDataNum, type, Base.DataInt, Base.DataDouble) { }
}

public partial class Parser
{
	static StateDataType GetStateDataType(int dataTypeInt)
		=> dataTypeInt switch
		{
			0 => StateDataType.Location,
			1 => StateDataType.Speed,
			2 => StateDataType.CurrentTime,
			3 => StateDataType.BCPressure,
			4 => StateDataType.MRPressure,
			5 => StateDataType.ERPressure,
			6 => StateDataType.BPPressure,
			7 => StateDataType.SAPPressure,
			8 => StateDataType.ElectricCurrent,
			9 => StateDataType.WireVoltage,
			10 => StateDataType.Time_Hour,
			11 => StateDataType.Time_Minute,
			12 => StateDataType.Time_Second,
			13 => StateDataType.Time_MilliSecond,

			-1 => StateDataType.AllData,
			-2 => StateDataType.PressureList,
			-3 => StateDataType.TimeInString,

			_ => StateDataType.Unknown,
		};
}
