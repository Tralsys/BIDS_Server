namespace BIDS.Parser;

public enum ErrorType
{
	NoError,
	Unknown,
	NotBIDSCmd,
	CannotParseToInt,
}

public record ParseError(ErrorType ErrorType) : IBIDSCmd
{
	public string ToCommandStr()
		=> throw new NotImplementedException();
}
