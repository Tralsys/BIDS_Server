using System.Collections.Generic;
using System.Linq;

using BIDS.Parser;
using BIDS.Parser.Variable;

using TR.VariableSMemMonitor.Core;

namespace TR;

public partial class BIDSServerCore
{
	const int VARIABLE_DATA_TYPE_ID_BIAS = 0x100;
	VariableSMemAutoReader Reader { get; }
	List<VariableStructure> VariableStructureList { get; } = new();

	/// <summary>
	/// SMemに書き込まれた値が更新された際に実行するメソッド
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Reader_ValueChanged(object? sender, VariableSMemWatcher.ChangedValues e)
		=> OnValueUpdateDetected(e);

	private void OnValueUpdateDetected(VariableSMemWatcher.ChangedValues e, IBIDSsv? senderMod = null)
	{
		VariableStructure structure
			= VariableStructureList.Find(v => v.Name == e.SMemName)
			?? OnNewNameDetected(e.Structure);

		IBIDSBinaryData payload = new VariablePayload(e.RawPayload, structure);
		byte[] cmd = payload.GetBytesWithHeader();

		foreach (var mod in _ServerParserDic.Keys)
		{
			if (mod != senderMod)
				mod.Print(cmd);
		}
	}

	/// <summary>
	/// Variable SMemが新しく作成された際に実行するメソッド
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Reader_NameAdded(object? sender, VariableSMemNameAddedEventArgs e)
		=> OnNewNameDetected(e.Structure);

	private VariableStructure OnNewNameDetected(VariableStructure structure, IBIDSsv? senderMod = null)
	{
		int index = VariableStructureList.Count;
		structure = structure with
		{
			DataTypeId = index + VARIABLE_DATA_TYPE_ID_BIAS
		};

		VariableStructureList.Add(structure);

		if (VariableStructureList[index] != structure)
		{
			index = VariableStructureList.IndexOf(structure);

			structure = structure with
			{
				DataTypeId = index + VARIABLE_DATA_TYPE_ID_BIAS
			};

			VariableStructureList[index] = structure;
		}

		IBIDSBinaryData registerCmd = new VariableStructureRegister(structure);
		byte[] cmd = registerCmd.GetBytesWithHeader().ToArray();

		foreach (var mod in _ServerParserDic.Keys)
		{
			if (mod != senderMod)
				mod.Print(cmd);
		}

		return structure;
	}

	// TODO: NO_SMEM_MODEへの対応

	public byte[]? OnVariableStructureRegisterCmdGot(VariableStructure structure, IBIDSsv mod)
	{
		// TODO: 既に保持しているSMemと受信した構造が一致するかどうかのチェックが必要
		if (VariableStructureList.Find(v => v.Name == structure.Name) is not null)
			return null;

		structure = OnNewNameDetected(structure, mod);

		VariableSMemWatcher.ChangedValues? changedValues = Reader.AddNewStructure(structure);

		if (changedValues is not null)
			OnValueUpdateDetected(changedValues, mod);

		return null;
	}

	public byte[]? OnVariablePayloadCmdGot(string name, VariableStructurePayload Payload, IBIDSsv mod)
	{
		VariableStructure? structure = VariableStructureList.Find(v => v.Name == name);

		if (structure is null)
			return (new BIDSBinaryData_Error(BIDSBinaryDataErrorType.UnknownVariableDataKey) as IBIDSBinaryData).GetBytesWithHeader();

		VariableSMemWatcher.ChangedValues? changedValues = Reader.AddNewStructure(structure);

		if (changedValues is not null)
			OnValueUpdateDetected(changedValues, mod);

		return null;
	}

}
