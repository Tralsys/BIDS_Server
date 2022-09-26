namespace BIDS.Parser.VariableCommands;

public class VariableStructurePayload : Dictionary<string, VariableStructure.IDataRecord>, IValiableCmdResult
{
	public int DataTypeId { get; }

	public VariableStructurePayload(int dataTypeId)
	{
		DataTypeId = dataTypeId;
	}
}
