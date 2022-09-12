namespace BIDS.Parser;

public interface IParser
{
	IBIDSCmd From(string str);
	IBIDSCmd From(ReadOnlySpan<char> str);
}
