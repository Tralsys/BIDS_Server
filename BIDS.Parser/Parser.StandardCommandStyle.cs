namespace BIDS.Parser;

public record BIDSCmd_PowerControl(
	int Value
	) : IBIDSCmd_StandardStyle
{
	public string ToCommandStr()
		=> $"TRP{Value}";
}

public record BIDSCmd_BrakeControl(
	int Value
	) : IBIDSCmd_StandardStyle
{
	public string ToCommandStr()
		=> $"TRB{Value}";
}

public record BIDSCmd_OneHandleControl(
	int Value
	) : IBIDSCmd_StandardStyle
{
	public string ToCommandStr()
		=> $"TRB{Value}";
}

public partial class Parser
{
	static IBIDSCmd ParseStandardCommandStyle(in ReadOnlySpan<char> str, Identifier type)
	{
		var err = ValidateAndPickDataInt(str, out var gotData);
		if (err is not null)
			return err;

		if (!int.TryParse(str, out int pos))
			return new ParseError(ErrorType.CannotParseToInt);

		return type switch
		{
			Identifier.ControlPower => new BIDSCmd_PowerControl(pos),
			Identifier.ControlBrake => new BIDSCmd_BrakeControl(pos),
			Identifier.ControlOnehandle => new BIDSCmd_OneHandleControl(pos),
			_ => throw new KeyNotFoundException()
		};
	}
}
