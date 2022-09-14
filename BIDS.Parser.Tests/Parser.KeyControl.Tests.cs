using NUnit.Framework;
using BIDS.Parser;
using System.Collections.Generic;
using System;

namespace BIDS.Parser.Tests;

public class Parser_KeyControlTests
{
	// Valid Cases
	[TestCase("TRKP0", 0, KeyType.Horn1, KeyControlType.Pressed)]
	[TestCase("TRKR0", 0, KeyType.Horn1, KeyControlType.Released)]
	[TestCase("TRKP1", 1, KeyType.Horn2, KeyControlType.Pressed)]
	[TestCase("TRKP2", 2, KeyType.MusicHorn, KeyControlType.Pressed)]
	[TestCase("TRKP3", 3, KeyType.ConstSpeed, KeyControlType.Pressed)]
	[TestCase("TRKP4", 4, KeyType.ATS_S, KeyControlType.Pressed)]
	[TestCase("TRKP5", 5, KeyType.ATS_A1, KeyControlType.Pressed)]
	[TestCase("TRKP6", 6, KeyType.ATS_A2, KeyControlType.Pressed)]
	[TestCase("TRKP7", 7, KeyType.ATS_B1, KeyControlType.Pressed)]
	[TestCase("TRKP8", 8, KeyType.ATS_B2, KeyControlType.Pressed)]
	[TestCase("TRKP9", 9, KeyType.ATS_C1, KeyControlType.Pressed)]
	[TestCase("TRKP10", 10, KeyType.ATS_C2, KeyControlType.Pressed)]
	[TestCase("TRKP11", 11, KeyType.ATS_D, KeyControlType.Pressed)]
	[TestCase("TRKP12", 12, KeyType.ATS_E, KeyControlType.Pressed)]
	[TestCase("TRKP13", 13, KeyType.ATS_F, KeyControlType.Pressed)]
	[TestCase("TRKP14", 14, KeyType.ATS_G, KeyControlType.Pressed)]
	[TestCase("TRKP15", 15, KeyType.ATS_H, KeyControlType.Pressed)]
	[TestCase("TRKP16", 16, KeyType.ATS_I, KeyControlType.Pressed)]
	[TestCase("TRKP17", 17, KeyType.ATS_J, KeyControlType.Pressed)]
	[TestCase("TRKP18", 18, KeyType.ATS_K, KeyControlType.Pressed)]
	[TestCase("TRKP19", 19, KeyType.ATS_L, KeyControlType.Pressed)]
	[TestCase("TRKP2147483647", 2147483647, KeyControlType.Unknown, KeyControlType.Pressed)]
	[TestCase("TRKR-2147483648", -2147483648, KeyControlType.Unknown, KeyControlType.Released)]
	[TestCase("TRK-1", -1, KeyType.Unknown, KeyControlType.Unknown)]

	[TestCase("TRK2147483647", 2147483647, KeyType.Unknown, KeyControlType.Unknown)]
	[TestCase("TRK-2147483648", -2147483648, KeyType.Unknown, KeyControlType.Unknown)]
	[TestCase("TRK?2147483647", 2147483647, KeyType.Unknown, KeyControlType.Unknown)]
	[TestCase("TRK?-2147483648", -2147483648, KeyType.Unknown, KeyControlType.Unknown)]
	public void ControlTests(string inputCmd, int expectedValue, KeyType expectedType, KeyControlType expectedControlType)
	{
		BIDSCmd_KeyControl expected = new(
			expectedType,
			expectedValue,
			expectedControlType,
			null
			);

		Assert.That(Parser.Default.From(inputCmd.AsSpan()), Is.EqualTo(expected));
	}

	// Valid Cases
	[TestCase("TRKP0X0", 0, KeyType.Horn1, KeyControlType.Pressed, 0)]
	[TestCase("TRKR0X0", 0, KeyType.Horn1, KeyControlType.Released, 0)]
	[TestCase("TRK0X0", 0, KeyType.Horn1, KeyControlType.Unknown, 0)]
	public void DataIntTests(string inputCmd, int expectedValue, KeyType expectedType, KeyControlType expectedControlType, int expectedReturnValue)
	{
		BIDSCmd_KeyControl expected = new(
			expectedType,
			expectedValue,
			expectedControlType,
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

	[TestCase("TRK", ErrorType.NotBIDSCmd)]
	[TestCase("TRKP", ErrorType.CannotParseToInt)]
	[TestCase("TRKR", ErrorType.CannotParseToInt)]
	[TestCase("TRKP2147483648", ErrorType.CannotParseToInt)]
	[TestCase("TRKP-2147483649", ErrorType.CannotParseToInt)]
	public void ErrorTests(string inputCmd, ErrorType errType)
		=> Assert.That(Parser.Default.From(inputCmd), Is.EqualTo(new ParseError(errType)));
}
