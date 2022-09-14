namespace BIDS.Parser;

public record BIDSCmd_PowerControl(
	int Value,
	IReadOnlyList<int>? DataInt
	) : IBIDSCmd_StandardStyle
{
	public string ToCommandStr()
		=> $"TRP{Value}";
}

public record BIDSCmd_BrakeControl(
	int Value,
	IReadOnlyList<int>? DataInt
	) : IBIDSCmd_StandardStyle
{
	public string ToCommandStr()
		=> $"TRB{Value}";
}

public record BIDSCmd_OneHandleControl(
	int Value,
	IReadOnlyList<int>? DataInt
	) : IBIDSCmd_StandardStyle
{
	public string ToCommandStr()
		=> $"TRB{Value}";
}

public record BIDSCmd_VersionCheck(
	int Value,
	IReadOnlyList<int>? DataInt
	) : IBIDSCmd_StandardStyle
{
	public string ToCommandStr()
		=> $"TRV{Value}";
}

public record BIDSCmd_ErrorReport(
	int Value,
	IReadOnlyList<int>? DataInt
	) : IBIDSCmd_StandardStyle
{
	public string ToCommandStr()
		=> $"TRE{Value}";
}

public partial class Parser
{
	static IBIDSCmd ParseStandardCommandStyle(in ReadOnlySpan<char> str, Identifier type)
	{
		var err = ValidateAndPickDataInt(str, out var nonDataSpan, out var gotData);
		if (err is not null)
			return err;

		if (!int.TryParse(nonDataSpan, out int pos))
			return new ParseError(ErrorType.CannotParseToInt);

		return type switch
		{
			Identifier.ControlPower => new BIDSCmd_PowerControl(pos, gotData),
			Identifier.ControlBrake => new BIDSCmd_BrakeControl(pos, gotData),
			Identifier.ControlOnehandle => new BIDSCmd_OneHandleControl(pos, gotData),
			Identifier.Version => new BIDSCmd_VersionCheck(pos, gotData),
			Identifier.Error => new BIDSCmd_ErrorReport(pos, gotData),
			_ => throw new KeyNotFoundException()
		};
	}
}
