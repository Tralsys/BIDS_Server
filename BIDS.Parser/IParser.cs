namespace BIDS.Parser;

public interface IParser
{
	int Version { get; }

	IBIDSCmd From(string str);
	IBIDSCmd From(ReadOnlySpan<char> str);

	IBIDSCmd From(byte[] bytes) => From(bytes.AsSpan());
	IBIDSCmd From(ReadOnlySpan<byte> bytes);
}
