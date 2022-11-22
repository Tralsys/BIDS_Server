using System.Collections.Generic;
using System.Linq;

using BIDS.Parser;
using BIDS.Parser.Variable;

using TR.BIDSSMemLib;

namespace TR;

public partial class BIDSServerCore
{
	VariableSMemNameManager VariableSMemNames { get; } = new();
	Dictionary<string, VariableSMem> VariableSMems { get; } = new();

	public Dictionary<string, VariableStructure.IDataRecord> DataRecords { get; } = new();

	// TODO: NO_SMEM_MODEへの対応

	public byte[]? OnVariableStructureRegisterCmdGot(VariableStructure Structure, IParser parser)
	{
		if (VariableSMemNames.FirstOrDefault(v => v.Name == Structure.Name) is null)
		{
			VariableSMemNames.AddName(Structure.Name);
		}

		// TODO: 既に保持しているSMemと受信した構造が一致するかどうかのチェックが必要
		if (VariableSMems.ContainsKey(Structure.Name))
			return null;

		// デフォルトのCapacityは 0x1000 = 4096 [byte] にする
		VariableSMems[Structure.Name] = new VariableSMem(Structure.Name, 0x1000, Structure);

		_VariableStructureDic[parser].Add(Structure.DataTypeId, Structure);

		return null;
	}

	public byte[]? OnVariablePayloadCmdGot(string name, VariableStructurePayload Payload)
	{
		if (!VariableSMems.TryGetValue(name, out VariableSMem? smem))
			throw new KeyNotFoundException($"The structure `{name}` not found");

		smem.WriteToSMemFromPayload(Payload);

		foreach (var v in Payload)
			DataRecords[v.Key] = v.Value;

		return null;
	}
}
