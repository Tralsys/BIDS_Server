using NUnit.Framework;
using BIDS.Parser;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BIDS.Parser.Tests;

public class Parser_InfoCmd_HandlePosition_Tests
{
	// Valid Cases
	[TestCase("TRIH0", 0, HandlePosType.Brake)]
	[TestCase("TRIH1", 1, HandlePosType.Power)]
	[TestCase("TRIH2", 2, HandlePosType.Reverser)]
	[TestCase("TRIH3", 3, HandlePosType.ConstSpeed)]
	[TestCase("TRIH4", 4, HandlePosType.Unknown)]
	[TestCase("TRIH-1", -1, HandlePosType.Unknown)]
	public void RequestTests(string inputCmd, int expectedRawDataNum, HandlePosType expectedDataType)
	{
		BIDSCmd_Info_HandlePosition expected = new(
			'H',
			expectedRawDataNum,
			expectedDataType,
			null,
			null
			);

		Assert.That(Parser.Default.From(inputCmd.AsSpan()), Is.EqualTo(expected));
	}

	// Valid Cases
	[TestCase("TRIH-1X1X2X3", -1, HandlePosType.Unknown, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRIH0X1X2X3", 0, HandlePosType.Brake, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRIH0X1.2X2.3X3.4", 0, HandlePosType.Brake, new int[] { 1, 2, 3 }, new double[] { 1.2, 2.3, 3.4 })]
	public void ResponseTests(string inputCmd, int expectedRawDataNum, HandlePosType expectedDataType, int[] expectDataInt, double[] expectDataDouble)
	{
		BIDSCmd_Info_HandlePosition expected = new(
			'H',
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

	[TestCase("TRIH", ErrorType.NotBIDSCmd)]
	[TestCase("TRIH2147483648", ErrorType.CannotParseToInt)]
	[TestCase("TRIH-2147483649", ErrorType.CannotParseToInt)]
	public void ErrorTests(string inputCmd, ErrorType errType)
		=> Assert.That(Parser.Default.From(inputCmd), Is.EqualTo(new ParseError(errType)));
}
