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

		var responseToSend = OnDataGot(result, sv);

		if (responseToSend is not null)
			sv.Print(responseToSend);
	}

	private byte[]? OnDataGot(IBIDSCmd cmd, IBIDSsv sender)
	{
		string? returnCmd = null;

		switch (cmd)
		{
			case IBIDSCmd_Info infoCmd:
				if ((infoCmd.DataInt is null || infoCmd.DataInt.Count <= 0)
					&& (infoCmd.DataDouble is null || infoCmd.DataDouble.Count <= 0))
				{
					// データリクエスト型
					returnCmd = infoCmd.GenerateCommand(SMem.BIDSSMemData, SMem.PanelA, SMem.SoundA);
				}
				else
				{
					// 何らかのデータを持ったコマンド
					// => Responseコマンド
					SMem.WriteIsEnabled(true);
					returnCmd = OnResponseCmdGot.SetValue(SMem, infoCmd);
				}
				break;

			case IBIDSCmd_KeyControl or IBIDSCmd_ReverserControl or IBIDSCmd_StandardStyle:
				// Response
				// => 何らかの操作を行った結果の報告コマンド
				if (cmd is IBIDSCmd_HasDataInt v && v.DataInt?.Count > 0)
					return null;

				if (cmd is IStringBIDSCmd scmd)
					returnCmd = OnControlCmdGot.HandleRequest(SMem, scmd, Version);
				break;

			case IBIDSBinaryData.IVariableStructureRegister regCmd:
				return OnVariableStructureRegisterCmdGot(regCmd.Structure, sender);

			case IBIDSBinaryData.IVariablePayload payloadCmd:
				return OnVariablePayloadCmdGot(payloadCmd.Structure.Name, payloadCmd.Payload, sender);

			case IBIDSBinaryData binCmd:
				SMem.WriteIsEnabled(true);
				return OnBinaryCmdGot.HandleCommand(SMem, binCmd);
		}

		if (returnCmd is not null)
			return Encoding.ASCII.GetBytes(returnCmd);

		return null;
	}
}
