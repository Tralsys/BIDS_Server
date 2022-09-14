using NUnit.Framework;
using BIDS.Parser;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BIDS.Parser.Tests;

public class Parser_InfoCmd_SpecData_Tests
{
	// Valid Cases
	[TestCase("TRIC0", 0, SpecDataType.Brake)]
	[TestCase("TRIC1", 1, SpecDataType.Power)]
	[TestCase("TRIC2", 2, SpecDataType.ATSCheck)]
	[TestCase("TRIC3", 3, SpecDataType.B67)]
	[TestCase("TRIC4", 4, SpecDataType.CarCount)]
	[TestCase("TRIC5", 5, SpecDataType.Unknown)]
	[TestCase("TRIC-1", -1, SpecDataType.Unknown)]
	public void RequestTests(string inputCmd, int expectedRawDataNum, SpecDataType expectedDataType)
	{
		BIDSCmd_Info_SpecData expected = new(
			'C',
			expectedRawDataNum,
			expectedDataType,
			null,
			null
			);

		Assert.That(Parser.Default.From(inputCmd.AsSpan()), Is.EqualTo(expected));
	}

	// Valid Cases
	[TestCase("TRIC-1X1X2X3", -1, SpecDataType.Unknown, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRIC0X1X2X3", 0, SpecDataType.Brake, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRIC0X1.2X2.3X3.4", 0, SpecDataType.Brake, new int[] { 1, 2, 3 }, new double[] { 1.2, 2.3, 3.4 })]
	public void ResponseTests(string inputCmd, int expectedRawDataNum, SpecDataType expectedDataType, int[] expectDataInt, double[] expectDataDouble)
	{
		BIDSCmd_Info_SpecData expected = new(
			'C',
			expectedRawDataNum,
			expectedDataType,
			expectDataInt.ToList(),
			expectDataDouble.ToList()
			);

		if (Parser.Default.From(inputCmd.AsSpan()) is not IBIDSCmd_Info actualValue)
		{
			Assert.Fail($"{nameof(actualValue)} is not {typeof(IBIDSCmd_Info)}");
			return;
		}

		Assert.That(actualValue, Is.EqualTo(expected with { DataInt = actualValue.DataInt, DataDouble = actualValue.DataDouble }));
		Assert.That(actualValue.DataInt, Is.EquivalentTo(expected.DataInt));
		Assert.That(actualValue.DataDouble, Is.EquivalentTo(expected.DataDouble));
	}

	[TestCase("TRIC", ErrorType.NotBIDSCmd)]
	[TestCase("TRIC2147483648", ErrorType.CannotParseToInt)]
	[TestCase("TRIC-2147483649", ErrorType.CannotParseToInt)]
	public void ErrorTests(string inputCmd, ErrorType errType)
		=> Assert.That(Parser.Default.From(inputCmd), Is.EqualTo(new ParseError(errType)));
}
