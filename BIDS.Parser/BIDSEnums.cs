namespace BIDS.Parser;

/// <summary>コマンドの識別子</summary>
public enum Identifier
{
	/// <summary>不明なコマンド</summary>
	Unknown,

	/// <summary>レバーサー操作要求</summary>
	ControlReverser,

	/// <summary>ワンハンドル操作要求</summary>
	ControlOnehandle,

	/// <summary>力行ハンドル操作要求</summary>
	ControlPower,

	/// <summary>制動ハンドル操作要求</summary>
	ControlBrake,

	/// <summary>キー操作要求</summary>
	ControlKey,

	/// <summary>運転情報要求</summary>
	Information,

	/// <summary>バージョン情報要求</summary>
	Version,

	/// <summary>エラー通知</summary>
	Error,
}

/// <summary>情報データ種別</summary>
public enum DataType
{
	/// <summary>不明なデータ種別</summary>
	Unknown,

	SpecData,

	StateData,

	HandlePosition,

	PanelData,

	SoundData,

	DoorState,

	KeyPress,

	KeyRelease,
}

/// <summary>レバーサー位置設定</summary>
public enum ReverserPos
{
	Unknown,
	Forward,
	Neutral,
	Backward,
}

/// <summary>キー種類</summary>
public enum KeyType
{
	Unknown,
	Horn1,
	Horn2,
	MusicHorn,
	ConstSpeed,
	ATS_S,
	ATS_A1,
	ATS_A2,
	ATS_B1,
	ATS_B2,
	ATS_C1,
	ATS_C2,
	ATS_D,
	ATS_E,
	ATS_F,
	ATS_G,
	ATS_H,
	ATS_I,
	ATS_J,
	ATS_K,
	ATS_L,
}

public enum SpecDataType
{
	Unknown,
	AllData,
	Power,
	Brake,
	ATSCheck,
	B67,
	CarCount,
}

public enum StateDataType
{
	Unknown,
	AllData,
	PressureList,
	TimeInString,
	Location,
	Speed,
	CurrentTime,
	BCPressure,
	MRPressure,
	ERPressure,
	BPPressure,
	SAPPressure,
	ElectricCurrent,
	WireVoltage,
	Time_Hour,
	Time_Minute,
	Time_Second,
	Time_MilliSecond,
}

public enum HandlePosType
{
	Unknown,
	AllData,
	Brake,
	Power,
	Reverser,
	ConstSpeed,
	SelfBrake,
}

public enum DoorStateType
{
	Unknown,
	IsClosed,
}

public enum KeyControlType
{
	Unknown,
	Pressed,
	Released,
}
