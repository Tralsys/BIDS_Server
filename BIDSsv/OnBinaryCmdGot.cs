using System;
using BIDS.Parser;
using TR.BIDSSMemLib;

namespace TR.BIDSsv;

public static class OnBinaryCmdGot
{
	public static byte[]? HandleCommand(in ISMemLib SMem, in IBIDSBinaryData data)
		=> data switch
		{
			IBIDSBinaryData.ISpec v => HandleCommand(SMem, v),
			IBIDSBinaryData.IState v => HandleCommand(SMem, v),
			IBIDSBinaryData.IHandle v => HandleCommand(SMem, v),

			BIDSBinaryData_Panel v => HandleCommand(SMem, v),
			BIDSBinaryData_Sound v => HandleCommand(SMem, v),

			_ => null
		};

	public static byte[]? HandleCommand(in ISMemLib SMem, in IBIDSBinaryData.ISpec data)
	{
		// `Version`は、実装方針未定のため無視
		// `SelfBrake`、対応するフィールドがないため無視する
		SMem.Write(SMem.BIDSSMemData with
		{
			SpecData = new()
			{
				B = data.Brake,
				P = data.Power,
				A = data.ATSCheckPos,
				J = data.B67Pos,
				C = data.CarCount
			},
		});

		return null;
	}

	public static byte[]? HandleCommand(in ISMemLib SMem, in IBIDSBinaryData.IState data)
	{
		// `LineVoltage_V`と`NextSignalState`は、対応するフィールドが無いため無視する
		SMem.Write(SMem.BIDSSMemData with
		{
			StateData = new()
			{
				Z = data.Location_m,
				V = data.Speed_kmph,
				I = data.ElectricCurrent_A,
				T = data.Time_ms,
				BC = data.BCPressure_kPa,
				MR = data.MRPressure_kPa,
				ER = data.ERPressure_kPa,
				BP = data.BPPressure_kPa,
				SAP = data.SAPPressure_kPa,
			},
			IsDoorClosed = (data.IsDoorClosed == 1)
		});

		return null;
	}

	public static byte[]? HandleCommand(in ISMemLib SMem, in IBIDSBinaryData.IHandle data)
	{
		// `SelfBrake`は、対応するフィールドがないため無視する
		SMem.Write(SMem.BIDSSMemData with
		{
			HandleData = new()
			{
				P = data.Power,
				B = data.Brake,
				R = data.Reverser,
			}
		});

		return null;
	}

	public static byte[]? HandleCommand(in ISMemLib SMem, in BIDSBinaryData_Panel data)
	{
		int[] buf = SMem.ReadPanel();

		int bias = data.DataArray.Length * data.BiasNum;

		for (int i = 0; i < data.DataArray.Length; i++)
		{
			buf[i + bias] = data.DataArray[i];
		}

		SMem.WritePanel(buf);

		return null;
	}

	public static byte[]? HandleCommand(in ISMemLib SMem, in BIDSBinaryData_Sound data)
	{
		int[] buf = SMem.ReadSound();

		int bias = data.DataArray.Length * data.BiasNum;

		for (int i = 0; i < data.DataArray.Length; i++)
		{
			buf[i + bias] = data.DataArray[i];
		}

		SMem.WriteSound(buf);

		return null;
	}
}
