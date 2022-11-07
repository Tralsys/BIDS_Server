using NUnit.Framework;
using BIDS.Parser;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BIDS.Parser.Tests;

public class Parser_InfoCmd_StateData_Tests
{
	// Valid Cases
	[TestCase("TRIE0", 0, StateDataType.Location)]
	[TestCase("TRIE1", 1, StateDataType.Speed)]
	[TestCase("TRIE2", 2, StateDataType.CurrentTime)]
	[TestCase("TRIE3", 3, StateDataType.BCPressure)]
	[TestCase("TRIE4", 4, StateDataType.MRPressure)]
	[TestCase("TRIE5", 5, StateDataType.ERPressure)]
	[TestCase("TRIE6", 6, StateDataType.BPPressure)]
	[TestCase("TRIE7", 7, StateDataType.SAPPressure)]
	[TestCase("TRIE8", 8, StateDataType.ElectricCurrent)]
	[TestCase("TRIE9", 9, StateDataType.WireVoltage)]
	[TestCase("TRIE10", 10, StateDataType.Time_Hour)]
	[TestCase("TRIE11", 11, StateDataType.Time_Minute)]
	[TestCase("TRIE12", 12, StateDataType.Time_Second)]
	[TestCase("TRIE13", 13, StateDataType.Time_MilliSecond)]
	[TestCase("TRIE-1", -1, StateDataType.AllData)]
	[TestCase("TRIE-2", -2, StateDataType.PressureList)]
	[TestCase("TRIE-3", -3, StateDataType.TimeInString)]
	[TestCase("TRIE14", 14, StateDataType.Unknown)]
	[TestCase("TRIE-4", -4, StateDataType.Unknown)]
	public void RequestTests(string inputCmd, int expectedRawDataNum, StateDataType expectedDataType)
	{
		BIDSCmd_Info_StateData expected = new(
			'E',
			expectedRawDataNum,
			expectedDataType,
			null,
			null
			);

		Assert.That(Parser.Default.From(inputCmd.AsSpan()), Is.EqualTo(expected));
	}

	// Valid Cases
	[TestCase("TRIE-4X1X2X3", -4, StateDataType.Unknown, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRIE0X1X2X3", 0, StateDataType.Location, new int[] { 1, 2, 3 }, new double[] { 1, 2, 3 })]
	[TestCase("TRIE0X1.2X2.3X3.4", 0, StateDataType.Location, new int[] { 1, 2, 3 }, new double[] { 1.2, 2.3, 3.4 })]
	public void ResponseTests(string inputCmd, int expectedRawDataNum, StateDataType expectedDataType, int[] expectDataInt, double[] expectDataDouble)
	{
		BIDSCmd_Info_StateData expected = new(
			'E',
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

	[TestCase("TRIE", ErrorType.NotBIDSCmd)]
	[TestCase("TRIE2147483648", ErrorType.CannotParseToInt)]
	[TestCase("TRIE-2147483649", ErrorType.CannotParseToInt)]
	public void ErrorTests(string inputCmd, ErrorType errType)
		=> Assert.That(Parser.Default.From(inputCmd), Is.EqualTo(new ParseError(errType)));
}
