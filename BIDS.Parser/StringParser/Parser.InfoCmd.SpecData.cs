using BIDS.Parser.Internals;

namespace BIDS.Parser;

public record BIDSCmd_Info_SpecData(
	char RawDataType,
	int RawDataNum,
	SpecDataType SpecDataType,
	IReadOnlyList<int>? DataInt,
	IReadOnlyList<double>? DataDouble
) : BIDSCmd_Info(RawDataType, RawDataNum, DataInt, DataDouble), IBIDSCmd_Info_SpecData
{
	public BIDSCmd_Info_SpecData(BIDSCmd_Info Base, SpecDataType type)
		: this(Base.RawDataType, Base.RawDataNum, type, Base.DataInt, Base.DataDouble) { }
}

public partial class Parser
{
	static SpecDataType GetSpecDataType(int dataTypeInt)
		=> dataTypeInt switch
		{
			0 => SpecDataType.Brake,
			1 => SpecDataType.Power,
			2 => SpecDataType.ATSCheck,
			3 => SpecDataType.B67,
			4 => SpecDataType.CarCount,

			-1 => SpecDataType.AllData,

			_ => SpecDataType.Unknown,
		};
}
