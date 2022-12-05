using BIDS.Parser.Internals;
using BIDS.Parser.Variable;

namespace BIDS.Parser;

public partial class Parser : IParser
{
	public int Version => 100;

	public static Parser Default { get; } = new();

	BinaryParser BinaryParser { get; }

	public Parser() : this(new Dictionary<int, VariableStructure>()) { }

	public Parser(IReadOnlyDictionary<int, VariableStructure> dataTypeDict)
	{
		BinaryParser = new(dataTypeDict);
	}

	public IStringBIDSCmd From(string str)
		=> From(str.AsSpan());
	public IStringBIDSCmd From(ReadOnlySpan<char> str)
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

	public IBIDSCmd From(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length >= 4 && (char)bytes[0] == 'T' && (char)bytes[1] == 'R')
		{
			char[] str = new char[bytes.Length];

			System.Text.Encoding.ASCII.GetChars(bytes, str.AsSpan());

			return From(str.AsSpan());
		}
		else
		{
			return BinaryParser.From(bytes);
		}
	}

	static IStringBIDSCmd? ValidateAndPickDataInt(in ReadOnlySpan<char> str, out ReadOnlySpan<char> nonDataSpan, out List<int>? gotData)
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
