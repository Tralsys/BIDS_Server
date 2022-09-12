using NUnit.Framework;
using BIDS.Parser;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BIDS.Parser.Tests;

public class Parser_InfoCmd_PanelData_Tests
{
	// Valid Cases
	[TestCase("TRIP0", 0, null, null)]
	[TestCase("TRIP1", 1, null, null)]
	[TestCase("TRIP-1", -1, null, null)]
	[TestCase("TRIP-1X1X2X3", -1, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRIP0X1X2X3", 0, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRIP0X1.2X2.3X3.4", 0, new int[] { 1, 2, 3 }, new double[] { 1.2, 2.3, 3.4 })]
	public void Tests(string inputCmd, int expectedRawDataNum, int[] expectDataInt, double[] expectDataDouble)
	{
		BIDSCmd_Info_PanelData expected = new(
			'P',
			expectedRawDataNum,
			new List<int>() { expectedRawDataNum },
			expectDataInt?.ToList(),
			expectDataDouble?.ToList()
			);

		if (Parser.Default.From(inputCmd.AsSpan()) is not IBIDSCmd_Info_ArrayData actualValue)
		{
			Assert.Fail($"{nameof(actualValue)} is not {typeof(IBIDSCmd_Info_ArrayData)}");
			return;
		}

		Assert.That(actualValue, Is.EqualTo(expected with
		{
			IndexList = actualValue.IndexList,
			DataInt = actualValue.DataInt,
			DataDouble = actualValue.DataDouble
		}));
		Assert.That(actualValue.IndexList, Is.EquivalentTo(expected.IndexList));
		Assert.That(actualValue.DataInt, Is.EqualTo(expected.DataInt).Or.EquivalentTo(expected.DataInt));
		Assert.That(actualValue.DataDouble, Is.EqualTo(expected.DataDouble).Or.EquivalentTo(expected.DataDouble));
	}

	[TestCase("TRIP", ErrorType.NotBIDSCmd)]
	[TestCase("TRIP2147483648", ErrorType.CannotParseToInt)]
	[TestCase("TRIP-2147483649", ErrorType.CannotParseToInt)]
	public void ErrorTests(string inputCmd, ErrorType errType)
		=> Assert.That(Parser.Default.From(inputCmd), Is.EqualTo(new ParseError(errType)));
}
