using BIDS.Parser.Variable;

namespace BIDS.Parser;

public interface IParser
{
	int Version { get; }

	IStringBIDSCmd From(string str);
	IStringBIDSCmd From(ReadOnlySpan<char> str);

	IBIDSCmd From(byte[] bytes) => From(bytes.AsSpan());
	IBIDSCmd From(ReadOnlySpan<byte> bytes);
}

public interface IBinaryParser
{
	VariableCmdParser VariableCmdParser { get; }

	IBIDSBinaryData From(byte[] bytes) => From(bytes.AsSpan());
	IBIDSBinaryData From(ReadOnlySpan<byte> bytes);
}
