namespace BIDS.Parser;

public enum ErrorType
{
	NoError,
	Unknown,
	NotBIDSCmd,
	CannotParseToInt,
}

public record ParseError(ErrorType ErrorType) : IStringBIDSCmd
{
	public string ToCommandStr()
		=> throw new NotImplementedException();
}
