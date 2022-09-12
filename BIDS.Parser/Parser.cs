using BIDS.Parser.Internals;

namespace BIDS.Parser;

public partial class Parser : IParser
{
	public static Parser Default { get; } = new();

	public IBIDSCmd From(string str)
		=> From(str.AsSpan());
	public IBIDSCmd From(ReadOnlySpan<char> str)
	{
		if (str.Length < 4 || str[0] != 'T' || str[1] != 'R')
			return new ParseError(ErrorType.NotBIDSCmd);

		return str[2] switch
		{
			'R' => ControlReverser(str[3..]),

			'S' => ParseStandardCommandStyle(str[3..], Identifier.ControlOnehandle),
			'P' => ParseStandardCommandStyle(str[3..], Identifier.ControlPower),
			'B' => ParseStandardCommandStyle(str[3..], Identifier.ControlBrake),

			'K' => ControlKey(str[3..]),

			'I' => InfoCmd(str[3..]),

			'V' => ParseStandardCommandStyle(str[3..], Identifier.Version),

			'E' => ParseStandardCommandStyle(str[3..], Identifier.Error),

			_ => new ParseError(ErrorType.NotBIDSCmd)
		};
	}

	static IBIDSCmd? ValidateAndPickDataInt(in ReadOnlySpan<char> str, out ReadOnlySpan<char> nonDataSpan, out List<int>? gotData)
	{
		gotData = null;
		nonDataSpan = str;

		if (str.Length < 1)
			return new ParseError(ErrorType.NotBIDSCmd);

		int index = str.IndexOf('X');
		if (index > 0)
		{
			gotData = ValueListGetters.GetIntList(str[(index + 1)..]);

			nonDataSpan = str[..index];
		}

		return null;
	}
}
