using NUnit.Framework;
using BIDS.Parser;
using System.Collections.Generic;
using System;

namespace BIDS.Parser.Tests;

public class Parser_PowerControlTests
{
	// Valid Cases
	[TestCase("TRP-1", -1)]
	[TestCase("TRP0", 0)]
	[TestCase("TRP1", 1)]
	[TestCase("TRP2", 2)]
	[TestCase("TRP-2", -2)]
	[TestCase("TRP2147483647", 2147483647)]
	[TestCase("TRP-2147483648", -2147483648)]
	public void ControlTests(string inputCmd, int expectedValue)
	{
		BIDSCmd_PowerControl expected = new(
			expectedValue,
			null
			);

		Assert.That(Parser.Default.From(inputCmd.AsSpan()), Is.EqualTo(expected));
	}

	// Valid Cases
	[TestCase("TRP-1X0", -1, 0)]
	[TestCase("TRP0X0", 0, 0)]
	[TestCase("TRP1X0", 1, 0)]
	[TestCase("TRP2X0", 2, 0)]
	[TestCase("TRP-2X0", -2, 0)]
	[TestCase("TRP2147483647X0", 2147483647, 0)]
	[TestCase("TRP-2147483648X0", -2147483648, 0)]
	public void DataIntTests(string inputCmd, int expectedValue, int expectedReturnValue)
	{
		BIDSCmd_PowerControl expected = new(
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

	[TestCase("TRP", ErrorType.NotBIDSCmd)]
	[TestCase("TRP2147483648", ErrorType.CannotParseToInt)]
	[TestCase("TRP-2147483649", ErrorType.CannotParseToInt)]
	public void ErrorTests(string inputCmd, ErrorType errType)
		=> Assert.That(Parser.Default.From(inputCmd), Is.EqualTo(new ParseError(errType)));
}
