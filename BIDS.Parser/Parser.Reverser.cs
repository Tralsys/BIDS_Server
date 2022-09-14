namespace BIDS.Parser;

public record BIDSCmd_ReverserControl(
	ReverserPos ReverserPos,
	int? Value,
	IReadOnlyList<int>? DataInt
) : IBIDSCmd_ReverserControl
{
	public string ToCommandStr()
		=> "TRR" + (
		Value?.ToString() ?? ReverserPos switch
		{
			ReverserPos.Forward => "F",
			ReverserPos.Neutral => "N",
			ReverserPos.Backward => "B",
			_ => throw new NotSupportedException($"{ToCommandStr} Method is not supported when the value of {ReverserPos} is {ReverserPos.Unknown}")
		});
}

public partial class Parser
{
	static IBIDSCmd ControlReverser(in ReadOnlySpan<char> str)
	{
		var err = ValidateAndPickDataInt(str, out var nonDataSpan, out var gotData);
		if (err is not null)
			return err;

		bool isNumber = int.TryParse(nonDataSpan, out int num);

		ReverserPos pos = isNumber
			? num switch
			{
				< 0 => ReverserPos.Backward,
				> 0 => ReverserPos.Forward,
				_ => ReverserPos.Neutral

			}
			: str[0] switch
			{
				'F' => ReverserPos.Forward,
				'N' => ReverserPos.Neutral,
				'B' or 'R' => ReverserPos.Backward,
				_ => ReverserPos.Unknown
			};

		return new BIDSCmd_ReverserControl(pos, isNumber ? num : null, gotData);
	}
}

