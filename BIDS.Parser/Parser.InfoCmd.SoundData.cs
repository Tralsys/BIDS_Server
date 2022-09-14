namespace BIDS.Parser;

public record BIDSCmd_Info_SoundData(
	char RawDataType,
	int RawDataNum,
	IReadOnlyList<int> IndexList,
	IReadOnlyList<int>? DataInt,
	IReadOnlyList<double>? DataDouble
) : BIDSCmd_Info(RawDataType, RawDataNum, DataInt, DataDouble), IBIDSCmd_Info_SoundData
{
	public BIDSCmd_Info_SoundData(
		char RawDataType,
		int RawDataNum,
		int Index,
		IReadOnlyList<int> DataInt,
		IReadOnlyList<double> DataDouble
	) : this(RawDataType, RawDataNum, new List<int>() { Index }, DataInt, DataDouble) { }

	public BIDSCmd_Info_SoundData(BIDSCmd_Info Base, IReadOnlyList<int> IndexList)
		: this(Base.RawDataType, Base.RawDataNum, IndexList, Base.DataInt, Base.DataDouble) { }

	public BIDSCmd_Info_SoundData(BIDSCmd_Info Base, int Index)
		: this(Base, new List<int>() { Index }) { }
}
