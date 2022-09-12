using NUnit.Framework;
using BIDS.Parser;
using System.Collections.Generic;
using System;

namespace BIDS.Parser.Tests;

public class Parser_ReverserTests
{
	// Valid Cases
	[TestCase("TRRR", ReverserPos.Backward, null)]
	[TestCase("TRRB", ReverserPos.Backward, null)]
	[TestCase("TRRN", ReverserPos.Neutral, null)]
	[TestCase("TRRF", ReverserPos.Forward, null)]
	[TestCase("TRR-1", ReverserPos.Backward, -1)]
	[TestCase("TRR0", ReverserPos.Neutral, 0)]
	[TestCase("TRR1", ReverserPos.Forward, 1)]

	// Invalid (But No Error) Cases => for future purposes
	[TestCase("TRR2", ReverserPos.Forward, 2)]
	[TestCase("TRR-2", ReverserPos.Backward, -2)]
	[TestCase("TRR2147483647", ReverserPos.Forward, 2147483647)]
	[TestCase("TRR-2147483648", ReverserPos.Backward, -2147483648)]
	public void ControlTests(string inputCmd, ReverserPos expectedReverserPos, int? expectedValue)
	{
		BIDSCmd_ReverserControl expected = new(
			ReverserPos: expectedReverserPos,
			Value: expectedValue,
			null
			);

		Assert.That(Parser.Default.From(inputCmd.AsSpan()), Is.EqualTo(expected));
	}

	// Valid Cases
	[TestCase("TRRRX0", ReverserPos.Backward, null, 0)]
	[TestCase("TRRBX0", ReverserPos.Backward, null, 0)]
	[TestCase("TRRNX0", ReverserPos.Neutral, null, 0)]
	[TestCase("TRRFX0", ReverserPos.Forward, null, 0)]
	[TestCase("TRR-1X0", ReverserPos.Backward, -1, 0)]
	[TestCase("TRR0X0", ReverserPos.Neutral, 0, 0)]
	[TestCase("TRR1X0", ReverserPos.Forward, 1, 0)]

	// Invalid (But No Error) Cases => for future purposes
	[TestCase("TRR2X0", ReverserPos.Forward, 2, 0)]
	[TestCase("TRR-2X0", ReverserPos.Backward, -2, 0)]
	[TestCase("TRR2147483647X0", ReverserPos.Forward, 2147483647, 0)]
	[TestCase("TRR-2147483648X0", ReverserPos.Backward, -2147483648, 0)]
	public void DataIntTests(string inputCmd, ReverserPos expectedReverserPos, int? expectedValue, int expectedReturnValue)
	{
		BIDSCmd_ReverserControl expected = new(
			ReverserPos: expectedReverserPos,
			Value: expectedValue,
			new List<int>() { expectedReturnValue }
			);

		if (Parser.Default.From(inputCmd.AsSpan()) is not IBIDSCmd_HasDataInt actualValue)
		{
			Assert.Fail($"{nameof(actualValue)} is not {typeof(IBIDSCmd_HasDataInt)}");
			return;
		}

		Assert.That(actualValue, Is.EqualTo(expected with { DataInt = actualValue.DataInt }));
		Assert.That(actualValue.DataInt, Is.EquivalentTo(expected.DataInt));
	}

	[TestCase("TRR", ErrorType.NotBIDSCmd)]
	public void ErrorTests(string inputCmd, ErrorType errType)
		=> Assert.That(Parser.Default.From(inputCmd), Is.EqualTo(new ParseError(errType)));
}
