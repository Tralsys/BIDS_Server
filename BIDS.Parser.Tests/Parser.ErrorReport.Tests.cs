using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace BIDS.Parser.Tests;

public class Parser_ErrorReportTests
{
	[TestCase("TRE2", 2, null)]
	[TestCase("TRE2X3", 2, new int[] { 3 })]
	public void RequestTests(string inputCmd, int expectedValue, int[]? expectedDataInt)
	{
		BIDSCmd_ErrorReport expected = new(
			expectedValue,
			expectedDataInt
			);

		if (Parser.Default.From(inputCmd) is not BIDSCmd_ErrorReport actual)
		{
			Assert.Fail($"{nameof(actual)} is not {typeof(BIDSCmd_ErrorReport)}");
			return;
		}

		Assert.That(actual, Is.EqualTo(expected with { DataInt = actual.DataInt }));

		if (expected.DataInt is not null)
			Assert.That(actual.DataInt, Is.EquivalentTo(expected.DataInt));
	}
}
