using BIDS.Parser;

using TR.BIDSSMemLib;

namespace TR.BIDSsv;

public static class OnControlCmdGot
{
	public static string? HandleRequest(in ISMemLib _, in IBIDSCmd cmd, in int VersionNum)
		=> cmd switch
		{
			IBIDSCmd_KeyControl v => HandleRequest(_, v),
			IBIDSCmd_ReverserControl v => HandleRequest(_, v),
			BIDSCmd_BrakeControl v => HandleRequest(_, v),
			BIDSCmd_ErrorReport v => HandleRequest(_, v),
			BIDSCmd_OneHandleControl v => HandleRequest(_, v),
			BIDSCmd_PowerControl v => HandleRequest(_, v),
			BIDSCmd_VersionCheck v => HandleRequest(_, v, VersionNum),

			_ => Errors.ErrorNums.UnknownError.GetCMD(),
		};

	public static string? HandleRequest(in ISMemLib _, in IBIDSCmd_KeyControl cmd)
	{
		if (CtrlInput.KeyArrSizeMax <= cmd.KeyNumber)
			return Errors.ErrorNums.DNUM_ERROR.GetCMD();

		CtrlInput.SetIsKeyPushed(cmd.KeyNumber, cmd.ControlType == KeyControlType.Pressed);

		return $"{cmd.ToCommandStr()}{BIDSCmdGenerator.SeparatorChar}0";
	}

	public static string? HandleRequest(in ISMemLib _, in IBIDSCmd_ReverserControl cmd)
	{
		if (cmd.Value is not int v)
		{
			if (cmd.ReverserPos == ReverserPos.Unknown)
				return Errors.ErrorNums.DNUM_ERROR.GetCMD();

			v = cmd.ReverserPos switch
			{
				ReverserPos.Forward => 1,
				ReverserPos.Neutral => 0,
				ReverserPos.Backward => -1,

				// デフォルト値は、念のためForwardにする。
				// フェイルセーフ的には問題があるものの、そこまでの安全性を求める必要は無いため
				_ => 1,
			};
		}

		CtrlInput.SetHandD(CtrlInput.HandType.Reverser, v);

		return $"{cmd.ToCommandStr()}{BIDSCmdGenerator.SeparatorChar}0";
	}

	public static string? HandleRequest(in ISMemLib _, in BIDSCmd_BrakeControl cmd)
	{
		CtrlInput.SetHandD(CtrlInput.HandType.Brake, cmd.Value);

		return $"{cmd.ToCommandStr()}{BIDSCmdGenerator.SeparatorChar}0";
	}

	public static string? HandleRequest(in ISMemLib _, in BIDSCmd_ErrorReport cmd)
	{
	}

	public static string? HandleRequest(in ISMemLib _, in BIDSCmd_OneHandleControl cmd)
	{
		int Power = cmd.Value > 0 ? cmd.Value : 0;
		int Brake = cmd.Value < 0 ? -cmd.Value : 0;

		// 操作はブレーキが優先される(はず)ため、ブレーキを先に流す。
		// これにより、BからPに一気に移動させたときに、先にB0にさせることができる。
		CtrlInput.SetHandD(CtrlInput.HandType.Brake, Brake);
		CtrlInput.SetHandD(CtrlInput.HandType.Power, Power);

		return $"{cmd.ToCommandStr()}{BIDSCmdGenerator.SeparatorChar}0";
	}

	public static string? HandleRequest(in ISMemLib _, in BIDSCmd_PowerControl cmd)
	{
		CtrlInput.SetHandD(CtrlInput.HandType.Power, cmd.Value);

		return null;
	}

	public static string? HandleRequest(in ISMemLib _, in BIDSCmd_VersionCheck cmd, in int VersionNum)
	{
		// TODO: Implement HERE
		// どのような処理にするかは検討中

		return $"{cmd.ToCommandStr()}{BIDSCmdGenerator.SeparatorChar}{VersionNum}";
	}
}
