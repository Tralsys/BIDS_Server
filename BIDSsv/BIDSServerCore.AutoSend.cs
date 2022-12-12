using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BIDS.Parser;

using TR.BIDSSMemLib;
using TR.BIDSsv;

namespace TR;

public partial class BIDSServerCore
{
	private void ASPtr(byte[] ba)
	{
		if (ba?.Length > 0 && ServerParserDic.Count > 0)
			Parallel.ForEach(ServerParserDic.Keys, v => v.Print(ba));
	}

	Dictionary<IBIDSCmd_Info, List<IBIDSsv>> ASList { get; } = new();

	private void SMem_SMC_BSMDChanged(object? sender, ValueChangedEventArgs<BIDSSharedMemoryData> e)
	{
		if (ServerParserDic.Count <= 0)
			return;

		Parallel.ForEach(ServerParserDic.Keys, v => v.OnBSMDChanged(e.NewValue));

		var OD = SMem.OpenData;

		if (!Equals(e.OldValue.SpecData, e.NewValue.SpecData))
			ASPtr(GetBIDSBinaryData_Spec(e.NewValue.SpecData, Version, (short)OD.SelfBCount).GetBytesWithHeader());
		if (!Equals(e.OldValue.StateData, e.NewValue.StateData))
			ASPtr(GetBIDSBinaryData_State(e.NewValue.StateData, 0, e.NewValue.IsDoorClosed).GetBytesWithHeader());
		if (!Equals(e.NewValue.HandleData, e.OldValue.HandleData))
			ASPtr(GetBIDSBinaryData_Handle(e.NewValue.HandleData, OD.SelfBPosition).GetBytesWithHeader());

		Parallel.ForEach(ASList, (kvp) => ASListCheck_BSMD(kvp.Key, kvp.Value, e));
	}

	static void ASListCheck_BSMD(IBIDSCmd_Info gotCmd, List<IBIDSsv> requestingModList, ValueChangedEventArgs<BIDSSharedMemoryData> e)
	{
		if (requestingModList.Count <= 0)
			return;

		TimeSpan ots = TimeSpan.FromMilliseconds(e.OldValue.StateData.T);
		TimeSpan nts = TimeSpan.FromMilliseconds(e.NewValue.StateData.T);

		bool isUpdated = gotCmd switch
		{
			IBIDSCmd_Info_SpecData spec => spec.SpecDataType switch
			{
				SpecDataType.AllData => !Equals(e.OldValue.SpecData, e.NewValue.SpecData),
				SpecDataType.ATSCheck => e.OldValue.SpecData.A != e.NewValue.SpecData.A,
				SpecDataType.B67 => e.OldValue.SpecData.J != e.NewValue.SpecData.J,
				SpecDataType.Brake => e.OldValue.SpecData.B != e.NewValue.SpecData.B,
				SpecDataType.CarCount => e.OldValue.SpecData.C != e.NewValue.SpecData.C,
				SpecDataType.Power => e.OldValue.SpecData.P != e.NewValue.SpecData.P,
				_ => false
			},

			IBIDSCmd_Info_DoorState door => door.DoorStateType switch
			{
				DoorStateType.IsClosed => e.OldValue.IsDoorClosed != e.NewValue.IsDoorClosed,
				_ => false
			},

			IBIDSCmd_Info_StateData state => state.StateDataType switch
			{
				StateDataType.AllData => !Equals(e.OldValue.StateData, e.NewValue.StateData),
				StateDataType.BCPressure => e.OldValue.StateData.BC != e.NewValue.StateData.BC,
				StateDataType.BPPressure => e.OldValue.StateData.BP != e.NewValue.StateData.BP,
				StateDataType.ElectricCurrent => e.OldValue.StateData.I != e.NewValue.StateData.I,
				StateDataType.Location => e.OldValue.StateData.Z != e.NewValue.StateData.Z,
				StateDataType.ERPressure => e.OldValue.StateData.ER != e.NewValue.StateData.ER,
				StateDataType.MRPressure => e.OldValue.StateData.MR != e.NewValue.StateData.MR,
				StateDataType.PressureList => !UFunc.State_Pressure_IsSame(e.OldValue.StateData, e.NewValue.StateData),
				StateDataType.SAPPressure => e.OldValue.StateData.SAP != e.NewValue.StateData.SAP,
				StateDataType.Speed => e.OldValue.StateData.V != e.NewValue.StateData.V,
				StateDataType.CurrentTime or StateDataType.TimeInString => e.OldValue.StateData.T != e.NewValue.StateData.T,
				StateDataType.Time_Hour => ots.Hours != nts.Hours,
				StateDataType.Time_Minute => ots.Minutes != nts.Minutes,
				StateDataType.Time_MilliSecond => ots.Milliseconds != nts.Milliseconds,
				StateDataType.Time_Second => ots.Seconds != nts.Seconds,
				StateDataType.WireVoltage => false,
				_ => false
			},

			IBIDSCmd_Info_HandlePosition handpos => handpos.HandlePosType switch
			{
				HandlePosType.AllData => Equals(e.OldValue.HandleData, e.NewValue.HandleData),
				HandlePosType.Brake => e.OldValue.HandleData.B == e.NewValue.HandleData.B,
				HandlePosType.ConstSpeed => e.OldValue.HandleData.C == e.NewValue.HandleData.C,
				HandlePosType.Power => e.OldValue.HandleData.P == e.NewValue.HandleData.P,
				HandlePosType.Reverser => e.OldValue.HandleData.R == e.NewValue.HandleData.R,
				HandlePosType.SelfBrake => false,
				_ => false
			},

			_ => false
		};

		if (isUpdated)
		{
			string? cmd = gotCmd.GenerateCommand(e.NewValue);
			if (!string.IsNullOrEmpty(cmd))
				requestingModList.ForEach(v => v.Print(cmd));
		}
	}

	static IBIDSBinaryData GetBIDSBinaryData_Spec(in Spec v, in int version, in short selfBrake)
		=> new BIDSBinaryData_Spec(
			Version: version,
			Brake: (short)v.B,
			Power: (short)v.P,
			ATSCheckPos: (short)v.A,
			B67Pos: (short)v.J,
			CarCount: (short)v.C,
			SelfBrake: selfBrake
		);
	static IBIDSBinaryData GetBIDSBinaryData_State(in State v, in int SigNum, in bool isDoorClosed)
		=> new BIDSBinaryData_State(
			Location_m: v.Z,
			Speed_kmph: v.V,
			ElectricCurrent_A: v.I,
			LineVoltage_V: 0,
			Time_ms: v.T,
			BCPressure_kPa: v.BC,
			MRPressure_kPa: v.MR,
			ERPressure_kPa: v.ER,
			BPPressure_kPa: v.BP,
			SAPPressure_kPa: v.SAP,
			NextSignalState: SigNum,
			IsDoorClosed: (byte)(isDoorClosed ? 1 : 0)
		);
	static IBIDSBinaryData GetBIDSBinaryData_Handle(in Hand v, int selfBrake)
		=> new BIDSBinaryData_Handle(
			Power: v.P,
			Brake: v.B,
			Reverser: v.R,
			SelfBrake: selfBrake
		);
}
