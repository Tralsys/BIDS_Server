namespace BIDS.Parser;

public interface IParser
{
	int Version { get; }

	IBIDSCmd From(string str);
	IBIDSCmd From(ReadOnlySpan<char> str);
}
