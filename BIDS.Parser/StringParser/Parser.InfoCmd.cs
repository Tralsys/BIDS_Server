﻿using BIDS.Parser.Internals;

namespace BIDS.Parser;

public record BIDSCmd_Info(
	char RawDataType,
	int RawDataNum,
	IReadOnlyList<int>? DataInt,
	IReadOnlyList<double>? DataDouble
) : IBIDSCmd_Info
{
	public virtual string ToCommandStr()
		=> $"TRI{RawDataType}{RawDataNum}";
}

public partial class Parser
{
	static IStringBIDSCmd InfoCmd(in ReadOnlySpan<char> str)
	{
		if (str.Length < 2)
			return new ParseError(ErrorType.NotBIDSCmd);

		int index = str.IndexOf('X');
		ReadOnlySpan<char> dataTypeNumStr = index < 0 ? str[1..] : str[1..index];

		if (int.TryParse(dataTypeNumStr, out int dataTypeInt) != true)
			return new ParseError(ErrorType.CannotParseToInt);

		List<int>? gotDataInt = null;
		List<double>? gotDataDouble = null;
		if (index > 0)
		{
			var tmp = str[(index + 1)..];
			gotDataInt = ValueListGetters.GetIntList(tmp);
			gotDataDouble = ValueListGetters.GetDoubleList(tmp);
		}

		BIDSCmd_Info Base = new(str[0], dataTypeInt, gotDataInt, gotDataDouble);

		return str[0] switch
		{
			'C' => new BIDSCmd_Info_SpecData(Base, GetSpecDataType(dataTypeInt)),
			'E' => new BIDSCmd_Info_StateData(Base, GetStateDataType(dataTypeInt)),
			'H' => new BIDSCmd_Info_HandlePosition(Base, GetHandlePosType(dataTypeInt)),
			'P' => new BIDSCmd_Info_PanelData(Base, dataTypeInt),
			'S' => new BIDSCmd_Info_SoundData(Base, dataTypeInt),
			'D' => new BIDSCmd_Info_DoorState(Base, GetDoorStateType(dataTypeInt)),

			_ => Base
		};
	}
}
