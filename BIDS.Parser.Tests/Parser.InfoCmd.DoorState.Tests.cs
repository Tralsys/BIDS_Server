using NUnit.Framework;
using BIDS.Parser;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BIDS.Parser.Tests;

public class Parser_InfoCmd_DoorState_Tests
{
	// Valid Cases
	[TestCase("TRID0", 0, DoorStateType.IsClosed)]
	[TestCase("TRID1", 1, DoorStateType.Unknown)]
	[TestCase("TRID-1", -1, DoorStateType.Unknown)]
	public void RequestTests(string inputCmd, int expectedRawDataNum, DoorStateType expectedDataType)
	{
		BIDSCmd_Info_DoorState expected = new(
			'D',
			expectedRawDataNum,
			expectedDataType,
			null,
			null
			);

		Assert.That(Parser.Default.From(inputCmd.AsSpan()), Is.EqualTo(expected));
	}

	// Valid Cases
	[TestCase("TRID-1X1X2X3", -1, DoorStateType.Unknown, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRID0X1X2X3", 0, DoorStateType.IsClosed, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRID0X1.2X2.3X3.4", 0, DoorStateType.IsClosed, new int[] { 1, 2, 3 }, new double[] { 1.2, 2.3, 3.4 })]
	public void ResponseTests(string inputCmd, int expectedRawDataNum, DoorStateType expectedDataType, int[] expectDataInt, double[] expectDataDouble)
	{
		BIDSCmd_Info_DoorState expected = new(
			'D',
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

	[TestCase("TRID", ErrorType.NotBIDSCmd)]
	[TestCase("TRID2147483648", ErrorType.CannotParseToInt)]
	[TestCase("TRID-2147483649", ErrorType.CannotParseToInt)]
	public void ErrorTests(string inputCmd, ErrorType errType)
		=> Assert.That(Parser.Default.From(inputCmd), Is.EqualTo(new ParseError(errType)));
}
