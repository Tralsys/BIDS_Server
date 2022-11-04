using System;
using System.Text;

using BIDS.Parser;

using TR.BIDSsv;

namespace TR;

public partial class BIDSServerCore
{
	private void Mod_DataGot(object? sender, DataGotEventArgs e)
	{
		if (sender is not IBIDSsv sv)
			return;

		if (!ServerParserDic.TryGetValue(sv, out IParser? parser))
			return;

		var result = parser.From(e.Bytes);

		var responseToSend = OnDataGot(result);

		if (responseToSend is not null)
			sv.Print(responseToSend);
	}

	private byte[]? OnDataGot(IBIDSCmd cmd)
	{
		string? returnCmd;

		if (cmd is IBIDSCmd_Info infoCmd)
		{
			if ((infoCmd.DataInt is null || infoCmd.DataInt.Count <= 0)
				&& (infoCmd.DataDouble is null || infoCmd.DataDouble.Count <= 0))
			{
				// データリクエスト型
				returnCmd = cmd.GenerateCommand(SMem.BIDSSMemData, SMem.PanelA, SMem.SoundA);
			}
			else
			{
				// 何らかのデータを持ったコマンド
				// => Responseコマンド
				returnCmd = OnResponseCmdGot.SetValue(SMem, infoCmd);
			}
		}
		else if (cmd is IBIDSCmd_KeyControl or IBIDSCmd_ReverserControl or IBIDSCmd_StandardStyle)
		{
			// Response
			// => 何らかの操作を行った結果の報告コマンド
			if (cmd is IBIDSCmd_HasDataInt v && v.DataInt?.Count > 0)
				return null;

			returnCmd = OnControlCmdGot.HandleRequest(SMem, cmd, Version);
		}
		else
		{
			throw new NotImplementedException();
		}

		if (returnCmd is not null)
			return Encoding.ASCII.GetBytes(returnCmd);

		return null;
	}
}