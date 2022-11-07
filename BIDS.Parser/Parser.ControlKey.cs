namespace BIDS.Parser;

public record BIDSCmd_KeyControl(
	KeyType KeyType,
	int KeyNumber,
	KeyControlType ControlType,
	IReadOnlyList<int>? DataInt
	) : IBIDSCmd_KeyControl
{
	public string ToCommandStr()
		=> "TRK" + KeyNumber.ToString() + (ControlType switch
		{
			KeyControlType.Pressed => 'P',
			KeyControlType.Released => 'R',
			_ => throw new NotSupportedException($"{ToCommandStr} Method is not supported when the value of {this.ControlType} is {KeyControlType.Unknown}")
		});
}

public partial class Parser
{
	static IStringBIDSCmd ControlKey(in ReadOnlySpan<char> str)
	{
		var err = ValidateAndPickDataInt(str, out var nonDataSpan, out var gotData);
		if (err is not null)
			return err;

		var type = nonDataSpan[0] switch
		{
			'R' => KeyControlType.Released,
			'P' => KeyControlType.Pressed,
			_ => KeyControlType.Unknown,
		};

		if (((char.IsDigit(nonDataSpan[0]) || nonDataSpan[0] == '-') || !int.TryParse(nonDataSpan[1..], out int pos)) && !int.TryParse(nonDataSpan, out pos))
			return new ParseError(ErrorType.CannotParseToInt);

		KeyType keyType = KeyType.Unknown;
		if (0 <= pos && pos < 20)
			keyType = (KeyType)(pos + 1);

		return new BIDSCmd_KeyControl(keyType, pos, type, gotData);
	}
}
