using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace BIDS.Parser.Tests;

public class Parser_VersionCheckTests
{
	[TestCase("TRV202", 202, null)]
	[TestCase("TRV202X203", 202, new int[] { 203 })]
	public void RequestTests(string inputCmd, int expectedValue, int[]? expectedDataInt)
	{
		BIDSCmd_VersionCheck expected = new(
			expectedValue,
			expectedDataInt
			);

		if (Parser.Default.From(inputCmd) is not BIDSCmd_VersionCheck actual)
		{
			Assert.Fail($"{nameof(actual)} is not {typeof(BIDSCmd_VersionCheck)}");
			return;
		}

		Assert.That(actual, Is.EqualTo(expected with { DataInt = actual.DataInt }));

		if (expected.DataInt is not null)
			Assert.That(actual.DataInt, Is.EquivalentTo(expected.DataInt));
	}
}
