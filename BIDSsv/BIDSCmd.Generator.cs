#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using BIDS.Parser;
using TR.BIDSSMemLib;

namespace TR.BIDSsv;

public static class BIDSCmdGenerator
{
	public const char SeparatorChar = 'X';
	public static string? GenerateCommand(
		this IBIDSCmd cmd,
		in BIDSSharedMemoryData? bsmd = null,
		in int[]? panel = null,
		in int[]? sound = null
	) => cmd switch
	{
		IBIDSCmd_Info_SpecData v => v.GenerateCommand(bsmd: bsmd),
		IBIDSCmd_Info_DoorState v => v.GenerateCommand(bsmd: bsmd),
		IBIDSCmd_Info_StateData v => v.GenerateCommand(bsmd: bsmd),
		IBIDSCmd_Info_HandlePosition v => v.GenerateCommand(bsmd: bsmd),

		IBIDSCmd_Info_PanelData v => v.GenerateCommand(panel: panel),
		IBIDSCmd_Info_SoundData v => v.GenerateCommand(sound: sound),
		_ => null
	};

	public static string? GenerateCommand(
		this IBIDSCmd_Info_SpecData cmd,
		in BIDSSharedMemoryData? bsmd = null,
		in int[]? panel = null,
		in int[]? sound = null
	) => bsmd is BIDSSharedMemoryData v ? cmd.GenerateCommand(v.SpecData) : null;
	public static string? GenerateCommand(this IBIDSCmd_Info_SpecData cmd, in Spec spec)
	{
		object? retVal = cmd.SpecDataType switch
		{
			SpecDataType.AllData => new StringBuilder().AppendJoin(SeparatorChar, new object[]
				{
					spec.B,
					spec.P,
					spec.A,
					spec.J,
					spec.C
				}).ToString(),

			SpecDataType.ATSCheck => spec.A,
			SpecDataType.B67 => spec.J,
			SpecDataType.Brake => spec.B,
			SpecDataType.CarCount => spec.C,
			SpecDataType.Power => spec.P,
			_ => null,
		};

		if (retVal is null)
			return null;
		return $"{cmd.ToCommandStr()}{SeparatorChar}{retVal}";
	}

	public static string? GenerateCommand(
		this IBIDSCmd_Info_DoorState cmd,
		in BIDSSharedMemoryData? bsmd = null,
		in int[]? panel = null,
		in int[]? sound = null
	) => bsmd is BIDSSharedMemoryData v ? cmd.GenerateCommand(v.IsDoorClosed) : null;
	public static string? GenerateCommand(this IBIDSCmd_Info_DoorState cmd, in bool isClosed)
	{
		object? retVal = cmd.DoorStateType switch
		{
			DoorStateType.IsClosed => isClosed ? 1 : 0,

			_ => null,
		};

		if (retVal is null)
			return null;
		return $"{cmd.ToCommandStr()}{SeparatorChar}{retVal}";
	}

	public static string? GenerateCommand(
		this IBIDSCmd_Info_StateData cmd,
		in BIDSSharedMemoryData? bsmd = null,
		in int[]? panel = null,
		in int[]? sound = null
	) => bsmd is BIDSSharedMemoryData v ? cmd.GenerateCommand(v.StateData) : null;
	public static string? GenerateCommand(this IBIDSCmd_Info_StateData cmd, in State state)
	{
		object? retVal = cmd.StateDataType switch
		{
			StateDataType.AllData => new StringBuilder().AppendJoin(SeparatorChar, new object[]
				{
					state.Z,
					state.V,
					state.T,
					state.BC,
					state.MR,
					state.ER,
					state.BP,
					state.SAP,
					state.I,
					0,
				}).ToString(),
			StateDataType.PressureList => new StringBuilder().AppendJoin(SeparatorChar, new object[]
				{
					state.BC,
					state.MR,
					state.ER,
					state.BP,
					state.SAP,
				}).ToString(),
			StateDataType.TimeInString => TimeSpan.FromMilliseconds(state.T).ToString(@"h\:mm\:ss\.fff"),

			StateDataType.BCPressure => state.BC,
			StateDataType.BPPressure => state.BP,
			StateDataType.CurrentTime => state.T,
			StateDataType.ElectricCurrent => state.I,
			StateDataType.ERPressure => state.ER,
			StateDataType.Location => state.Z,
			StateDataType.MRPressure => state.MR,
			StateDataType.SAPPressure => state.SAP,
			StateDataType.Speed => state.V,
			StateDataType.Time_Hour => TimeSpan.FromMilliseconds(state.T).Hours,
			StateDataType.Time_MilliSecond => TimeSpan.FromMilliseconds(state.T).Milliseconds,
			StateDataType.Time_Minute => TimeSpan.FromMilliseconds(state.T).Minutes,
			StateDataType.Time_Second => TimeSpan.FromMilliseconds(state.T).Seconds,

			StateDataType.WireVoltage => null,
			_ => null,
		};

		if (retVal is null)
			return null;
		return $"{cmd.ToCommandStr()}{SeparatorChar}{retVal}";
	}

	public static string? GenerateCommand(
		this IBIDSCmd_Info_HandlePosition cmd,
		in BIDSSharedMemoryData? bsmd = null,
		in int[]? panel = null,
		in int[]? sound = null
	) => bsmd is BIDSSharedMemoryData v ? cmd.GenerateCommand(v.HandleData) : null;
	public static string? GenerateCommand(this IBIDSCmd_Info_HandlePosition cmd, in Hand hand)
	{
		object? retVal = cmd.HandlePosType switch
		{
			HandlePosType.AllData => new StringBuilder().AppendJoin(SeparatorChar, new object[]
				{
					hand.B,
					hand.P,
					hand.R,
					hand.C,
				}).ToString(),

			HandlePosType.Brake => hand.B,
			HandlePosType.ConstSpeed => hand.C,
			HandlePosType.Power => hand.P,
			HandlePosType.Reverser => hand.R,
			HandlePosType.SelfBrake => null,

			_ => null,
		};

		if (retVal is null)
			return null;
		return $"{cmd.ToCommandStr()}{SeparatorChar}{retVal}";
	}

	public static string? GenerateCommand(
		this IBIDSCmd_Info_PanelData cmd,
		in BIDSSharedMemoryData? bsmd = null,
		in int[]? panel = null,
		in int[]? sound = null
	) => panel is not null ? cmd.GenerateCommand(panel) : null;
	public static string? GenerateCommand(
		this IBIDSCmd_Info_SoundData cmd,
		in BIDSSharedMemoryData? bsmd = null,
		in int[]? panel = null,
		in int[]? sound = null
	) => sound is not null ? cmd.GenerateCommand(sound) : null;
	public static string? GenerateCommand(this IBIDSCmd_Info_ArrayData cmd, in int[] array)
	{
		List<int> values = new();
		foreach (int i in cmd.IndexList)
			values.Add(i < array.Length ? array[i] : 0);

		if (values.Count <= 0)
			return null;
		return $"{cmd.ToCommandStr()}{SeparatorChar}{new StringBuilder().AppendJoin(SeparatorChar, values)}";
	}
}
