using System;

using BIDS.Parser;

using TR.BIDSSMemLib;

namespace TR.BIDSsv;

public static class OnResponseCmdGot
{
	public static string? SetValue(in ISMemLib SMem, in IBIDSCmd_Info cmd)
		=> cmd switch
		{
			IBIDSCmd_Info_SpecData v => SetValue(SMem, v),
			IBIDSCmd_Info_DoorState v => SetValue(SMem, v),
			IBIDSCmd_Info_StateData v => SetValue(SMem, v),
			IBIDSCmd_Info_HandlePosition v => SetValue(SMem, v),
			IBIDSCmd_Info_PanelData v => SetValue(SMem, v),
			IBIDSCmd_Info_SoundData v => SetValue(SMem, v),
			_ => Errors.ErrorNums.UnknownError.GetCMD()
		};

	public static string? SetValue(in ISMemLib SMem, in IBIDSCmd_Info_SpecData cmd)
	{
		if (cmd.DataInt is null)
			return null;

		Spec? spec = null;
		switch (cmd.SpecDataType)
		{
			case SpecDataType.AllData:
				if (cmd.DataInt.Count < 5)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				spec = new()
				{
					B = cmd.DataInt[0],
					P = cmd.DataInt[1],
					A = cmd.DataInt[2],
					J = cmd.DataInt[3],
					C = cmd.DataInt[4],
				};
				break;

			case SpecDataType.ATSCheck:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				spec = SMem.BIDSSMemData.SpecData with
				{
					A = cmd.DataInt[0],
				};
				break;

			case SpecDataType.B67:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				spec = SMem.BIDSSMemData.SpecData with
				{
					J = cmd.DataInt[0],
				};
				break;

			case SpecDataType.Brake:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				spec = SMem.BIDSSMemData.SpecData with
				{
					B = cmd.DataInt[0],
				};
				break;

			case SpecDataType.CarCount:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				spec = SMem.BIDSSMemData.SpecData with
				{
					C = cmd.DataInt[0],
				};
				break;

			case SpecDataType.Power:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				spec = SMem.BIDSSMemData.SpecData with
				{
					P = cmd.DataInt[0],
				};
				break;

			default:
				return Errors.ErrorNums.CMD_ERROR.GetCMD();
		}

		if (spec is Spec v)
		{
			SMem.Write(SMem.BIDSSMemData with
			{
				SpecData = v
			});
		}

		return null;
	}

	public static string? SetValue(in ISMemLib SMem, in IBIDSCmd_Info_DoorState cmd)
	{
		if (cmd.DataInt is null)
			return null;

		switch (cmd.DoorStateType)
		{
			case DoorStateType.IsClosed:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				SMem.Write(SMem.BIDSSMemData with
				{
					IsDoorClosed = (cmd.DataInt[0] == 1)
				});
				break;

			default:
				return Errors.ErrorNums.CMD_ERROR.GetCMD();
		}

		return null;
	}

	public static string? SetValue(in ISMemLib SMem, in IBIDSCmd_Info_StateData cmd)
	{
		if (cmd.DataInt is null || cmd.DataDouble is null)
			return null;

		State? state = null;
		switch (cmd.StateDataType)
		{
			case StateDataType.AllData:
				if (cmd.DataInt.Count < 10 && cmd.DataDouble.Count < 10)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = new State()
				{
					Z = cmd.DataDouble[0],
					V = (float)cmd.DataDouble[1],
					T = cmd.DataInt[2],
					BC = (float)cmd.DataDouble[3],
					MR = (float)cmd.DataDouble[4],
					ER = (float)cmd.DataDouble[5],
					BP = (float)cmd.DataDouble[6],
					SAP = (float)cmd.DataDouble[7],
					I = (float)cmd.DataDouble[8]
				};
				break;

			case StateDataType.PressureList:
				if (cmd.DataDouble.Count < 5)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					BC = (float)cmd.DataDouble[0],
					MR = (float)cmd.DataDouble[1],
					ER = (float)cmd.DataDouble[2],
					BP = (float)cmd.DataDouble[3],
					SAP = (float)cmd.DataDouble[4],
				};
				break;

			// TODO: RawDataを保持するフィールドをIBIDSCmdに追加する
			// (そうしないと、「整形された時刻」データをうまく取得できない)
			case StateDataType.TimeInString:
				throw new NotImplementedException();

			case StateDataType.BCPressure:
				if (cmd.DataDouble.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					BC = (float)cmd.DataDouble[0]
				};
				break;

			case StateDataType.BPPressure:
				if (cmd.DataDouble.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					BP = (float)cmd.DataDouble[0]
				};
				break;

			case StateDataType.CurrentTime:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					T = cmd.DataInt[0]
				};
				break;

			case StateDataType.ElectricCurrent:
				if (cmd.DataDouble.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					I = (float)cmd.DataDouble[0]
				};
				break;

			case StateDataType.ERPressure:
				if (cmd.DataDouble.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					ER = (float)cmd.DataDouble[0]
				};
				break;

			case StateDataType.Location:
				if (cmd.DataDouble.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					Z = cmd.DataDouble[0]
				};
				break;

			case StateDataType.MRPressure:
				if (cmd.DataDouble.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					MR = (float)cmd.DataDouble[0]
				};
				break;

			case StateDataType.SAPPressure:
				if (cmd.DataDouble.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					SAP = (float)cmd.DataDouble[0]
				};
				break;

			case StateDataType.Speed:
				if (cmd.DataDouble.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				state = SMem.BIDSSMemData.StateData with
				{
					V = (float)cmd.DataDouble[0]
				};
				break;

			case StateDataType.Time_Hour:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();
				else
				{
					state = SMem.BIDSSMemData.StateData;
					TimeSpan ts = TimeSpan.FromMilliseconds(state.Value.T);
					state = state.Value with
					{
						T = (int)(new TimeSpan(0, cmd.DataInt[0], ts.Minutes, ts.Seconds, ts.Milliseconds).TotalMilliseconds)
					};
				}
				break;

			case StateDataType.Time_Minute:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();
				else
				{
					state = SMem.BIDSSMemData.StateData;
					TimeSpan ts = TimeSpan.FromMilliseconds(state.Value.T);
					state = state.Value with
					{
						T = (int)(new TimeSpan(0, (int)ts.TotalHours, cmd.DataInt[0], ts.Seconds, ts.Milliseconds).TotalMilliseconds)
					};
				}
				break;

			case StateDataType.Time_Second:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();
				else
				{
					state = SMem.BIDSSMemData.StateData;
					TimeSpan ts = TimeSpan.FromMilliseconds(state.Value.T);
					state = state.Value with
					{
						T = (int)(new TimeSpan(0, (int)ts.TotalHours, ts.Minutes, cmd.DataInt[0], ts.Milliseconds).TotalMilliseconds)
					};
				}
				break;

			case StateDataType.Time_MilliSecond:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();
				else
				{
					state = SMem.BIDSSMemData.StateData;
					TimeSpan ts = TimeSpan.FromMilliseconds(state.Value.T);
					state = state.Value with
					{
						T = (int)(new TimeSpan(0, (int)ts.TotalHours, ts.Minutes, ts.Seconds, cmd.DataInt[0]).TotalMilliseconds)
					};
				}
				break;

			default:
				return Errors.ErrorNums.CMD_ERROR.GetCMD();
		}

		if (state is State v)
		{
			SMem.Write(SMem.BIDSSMemData with
			{
				StateData = v
			});
		}

		return null;
	}

	public static string? SetValue(in ISMemLib SMem, in IBIDSCmd_Info_HandlePosition cmd)
	{
		if (cmd.DataInt is null)
			return null;

		Hand? hand = null;
		switch (cmd.HandlePosType)
		{
			case HandlePosType.AllData:
				if (cmd.DataInt.Count < 5)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				hand = new()
				{
					B = cmd.DataInt[0],
					P = cmd.DataInt[1],
					R = cmd.DataInt[2],
					C = cmd.DataInt[3],
				};
				break;

			case HandlePosType.Brake:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				hand = SMem.BIDSSMemData.HandleData with
				{
					B = cmd.DataInt[0]
				};
				break;

			case HandlePosType.ConstSpeed:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				hand = SMem.BIDSSMemData.HandleData with
				{
					C = cmd.DataInt[0]
				};
				break;

			case HandlePosType.Power:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				hand = SMem.BIDSSMemData.HandleData with
				{
					P = cmd.DataInt[0]
				};
				break;

			case HandlePosType.Reverser:
				if (cmd.DataInt.Count < 1)
					return Errors.ErrorNums.CMD_ERROR.GetCMD();

				hand = SMem.BIDSSMemData.HandleData with
				{
					R = cmd.DataInt[0]
				};
				break;

			case HandlePosType.SelfBrake:
				return Errors.ErrorNums.CMD_ERROR.GetCMD();

			default:
				return Errors.ErrorNums.CMD_ERROR.GetCMD();
		}

		if (hand is Hand v)
		{
			SMem.Write(SMem.BIDSSMemData with
			{
				HandleData = v
			});
		}

		return null;
	}

	public static string? SetValue(in ISMemLib SMem, in IBIDSCmd_Info_PanelData cmd)
	{
		if (cmd.DataInt is null || cmd.DataInt.Count < 1)
			return null;

		if (cmd.RawDataNum is <= 0 and < 256)
		{
			SMem.SMC_PnlD[cmd.RawDataNum] = cmd.DataInt[0];
		}

		return null;
	}

	public static string? SetValue(in ISMemLib SMem, in IBIDSCmd_Info_SoundData cmd)
	{
		if (cmd.DataInt is null || cmd.DataInt.Count < 1)
			return null;

		if (cmd.RawDataNum is <= 0 and < 256)
		{
			SMem.SMC_SndD[cmd.RawDataNum] = cmd.DataInt[0];
		}

		return null;
	}
}
