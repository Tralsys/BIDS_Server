using BIDS.Parser.Internals;

namespace BIDS.Parser;

public static class Parser
{
	public static IBIDSCmd From(in string str)
		=> From(str.AsSpan());
	public static IBIDSCmd From(in ReadOnlySpan<char> str)
	{
		if (str.Length < 4 || str[0] != 'T' || str[1] != 'R')
			return new BIDSCmd(ParseError: new(ErrorType.NotBIDSCmd));

		return str[2] switch
		{
			'R' => ControlReverser(str[3..]),

			'S' => ParseStandardCommandStyle(str[3..], Identifier.ControlOnehandle),
			'P' => ParseStandardCommandStyle(str[3..], Identifier.ControlPower),
			'B' => ParseStandardCommandStyle(str[3..], Identifier.ControlBrake),

			'K' => ControlKey(str[3..]),

			'I' => str[3] switch
			{
				'C' => InfoCmd(str[4..], DataType.SpecData,
					v => new(SpecDataType: v switch
					{
						0 => SpecDataType.Brake,
						1 => SpecDataType.Power,
						2 => SpecDataType.ATSCheck,
						3 => SpecDataType.B67,
						4 => SpecDataType.CarCount,
						_ => SpecDataType.Unknown,
					})
				),

				'E' => InfoCmd(str[4..], DataType.StateData,
					v => new(StateDataType: v switch
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
						_ => StateDataType.Unknown,
					})
				),

				'H' => InfoCmd(str[4..], DataType.HandlePosition,
					v => new(HandlePosType: v switch
					{
						0 => HandlePosType.Brake,
						1 => HandlePosType.Power,
						2 => HandlePosType.Reverser,
						3 => HandlePosType.ConstSpeed,
						_ => HandlePosType.Unknown,
					})
				),

				'P' => InfoCmd(str[4..], DataType.PanelData),
				'S' => InfoCmd(str[4..], DataType.SoundData),
				'D' => InfoCmd(str[4..], DataType.DoorState,
					v => new(DoorStateType: v switch
					{
						0 => DoorStateType.IsOpen,
						_ => DoorStateType.Unknown,
					})
				),

				_ => new BIDSCmd(ParseError: new(ErrorType.NotBIDSCmd))
			},

			'V' => ParseStandardCommandStyle(str[3..], Identifier.Version),

			'E' => ParseStandardCommandStyle(str[3..], Identifier.Error),

			_ => new BIDSCmd(ParseError: new(ErrorType.NotBIDSCmd))
		};
	}

	static IBIDSCmd ControlReverser(in ReadOnlySpan<char> str)
	{
		var err = ValidateAndPickDataInt(str, out var data, out var gotData);
		if (err is not null)
			return err;

		ReverserPos pos = data[0] switch
		{
			'F' => ReverserPos.Forward,
			'N' => ReverserPos.Neutral,
			'B' or 'R' => ReverserPos.Backward,

			_ => int.TryParse(data, out int num) ? num switch
			{
				< 0 => ReverserPos.Backward,
				> 0 => ReverserPos.Forward,
				_ => ReverserPos.Neutral
			} : ReverserPos.Unknown
		};

		return new BIDSCmd(Identifier.ControlReverser, ReverserPos: pos, DataInt: gotData);
	}

	static IBIDSCmd ParseStandardCommandStyle(in ReadOnlySpan<char> str, Identifier type)
	{
		var err = ValidateAndPickDataInt(str, out var data, out var gotData);
		if (err is not null)
			return err;

		if (!int.TryParse(data, out int pos))
			return new BIDSCmd(ParseError: new(ErrorType.CannotParseToInt));

		return new BIDSCmd(type, RequestNumber: pos, DataInt: gotData);
	}

	static IBIDSCmd ControlKey(in ReadOnlySpan<char> str)
	{
		var err = ValidateAndPickDataInt(str, out var data, out var gotData);
		if (err is not null)
			return err;

		var type = data[0] switch
		{
			'R' => DataType.KeyRelease,
			'P' => DataType.KeyPress,
			_ => DataType.Unknown,
		};

		if (!int.TryParse(data[1..], out int pos))
			return new BIDSCmd(ParseError: new(ErrorType.CannotParseToInt));

		KeyType keyType = KeyType.Unknown;
		if (0 <= pos && pos < 20)
			keyType = (KeyType)(pos + 1);

		return new BIDSCmd(Identifier.ControlKey, DataType: type, KeyType: keyType, RequestNumber: pos, DataInt: gotData);
	}

	static IBIDSCmd InfoCmd(in ReadOnlySpan<char> str, DataType dataType, Func<int, BIDSCmd>? ReqNumParser = null)
	{
		if (str.Length < 1)
			return new BIDSCmd(ParseError: new(ErrorType.NotBIDSCmd));

		int index = str.IndexOf('X');
		ReadOnlySpan<char> dataTypeStr = index < 0 ? str : str[..index];

		if (int.TryParse(dataTypeStr, out int dataTypeInt) != true)
			return new BIDSCmd(ParseError: new(ErrorType.CannotParseToInt));

		List<int>? gotDataInt = null;
		List<double>? gotDataDouble = null;
		if (index > 0)
		{
			var tmp = str[(index + 1)..];
			gotDataInt = ValueListGetters.GetIntList(tmp);
			gotDataDouble = ValueListGetters.GetDoubleList(tmp);
		}

		return ReqNumParser?.Invoke(dataTypeInt) ?? new BIDSCmd() with
		{
			Identifier = Identifier.Information,
			DataType = dataType,
			RequestNumber = dataTypeInt,
			DataInt = gotDataInt,
			DataDouble = gotDataDouble,
		};
	}

	static IBIDSCmd? ValidateAndPickDataInt(in ReadOnlySpan<char> str, out ReadOnlySpan<char> data, out List<int>? gotData)
	{
		data = str;
		gotData = null;

		if (str.Length < 1)
			return new BIDSCmd(ParseError: new(ErrorType.NotBIDSCmd));

		int index = str.IndexOf('X');
		if (index > 0)
			gotData = ValueListGetters.GetIntList(str[(index + 1)..]);

		return null;
	}
}
