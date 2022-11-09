using System;
using BIDS.Parser.Variable;

namespace TR;

public partial class BIDSServerCore
{
	public byte[]? OnVariableStructureRegisterCmdGot(VariableStructure Structure)
	{
		throw new NotImplementedException();
	}

	public byte[]? OnVariablePayloadCmdGot(VariableStructure Structure, VariableStructurePayload Payload)
	{
		throw new NotImplementedException();
	}
}
