using NUnit.Framework;
using BIDS.Parser;
using System.Collections.Generic;
using System;

namespace BIDS.Parser.Tests;

public class Parser_OneHandleTests
{
	// Valid Cases
	[TestCase("TRS-1", -1)]
	[TestCase("TRS0", 0)]
	[TestCase("TRS1", 1)]
	[TestCase("TRS2", 2)]
	[TestCase("TRS-2", -2)]
	[TestCase("TRS2147483647", 2147483647)]
	[TestCase("TRS-2147483648", -2147483648)]
	public void ControlTests(string inputCmd, int expectedValue)
	{
		BIDSCmd_OneHandleControl expected = new(
			expectedValue,
			null
			);

		Assert.That(Parser.Default.From(inputCmd.AsSpan()), Is.EqualTo(expected));
	}

	// Valid Cases
	[TestCase("TRS-1X0", -1, 0)]
	[TestCase("TRS0X0", 0, 0)]
	[TestCase("TRS1X0", 1, 0)]
	[TestCase("TRS2X0", 2, 0)]
	[TestCase("TRS-2X0", -2, 0)]
	[TestCase("TRS2147483647X0", 2147483647, 0)]
	[TestCase("TRS-2147483648X0", -2147483648, 0)]
	public void DataIntTests(string inputCmd, int expectedValue, int expectedReturnValue)
	{
		BIDSCmd_OneHandleControl expected = new(
			expectedValue,
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

	[TestCase("TRS", ErrorType.NotBIDSCmd)]
	[TestCase("TRS2147483648", ErrorType.CannotParseToInt)]
	[TestCase("TRS-2147483649", ErrorType.CannotParseToInt)]
	public void ErrorTests(string inputCmd, ErrorType errType)
		=> Assert.That(Parser.Default.From(inputCmd), Is.EqualTo(new ParseError(errType)));
}
