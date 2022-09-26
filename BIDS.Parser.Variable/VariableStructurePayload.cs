using System.Collections.Generic;

namespace BIDS.Parser.Variable;

public class VariableStructurePayload : Dictionary<string, VariableStructure.IDataRecord>, IVariableCmdResult
{
	public int DataTypeId { get; }

	public VariableStructurePayload(int dataTypeId)
	{
		DataTypeId = dataTypeId;
	}
}
