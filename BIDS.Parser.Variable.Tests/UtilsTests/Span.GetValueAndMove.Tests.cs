namespace BIDS.Parser.Variable.Tests.UtilsTests;

public class Span_GetValueAndMoveTests
{
	static void TestUtil<T>(byte[] input, T expected, in ReadOnlySpan<byte> afterProcessingSpan, int size, T actual)
	{
		Assert.That(actual, Is.EqualTo(expected));

		Assert.That(afterProcessingSpan.ToArray(), Is.EquivalentTo(
#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
			input[size..]
#else
			input.Skip(size)
#endif
		));
	}

	[TestCase(new byte[] { 0, 1, 2, 3 }, false)]
	[TestCase(new byte[] { 1, 0, 2, 3 }, true)]
	[TestCase(new byte[] { 0xFF, 0, 2, 3 }, true)]
	public void BooleanTest(byte[] input, bool expected)
	{
		ReadOnlySpan<byte> bytes = input.ToArray().AsSpan();

		TestUtil(input, expected, bytes,
			1,
			Utils.GetBooleanAndMove(ref bytes)
		);
	}

	[TestCase(new byte[] { 1, 2, 3, 4 }, (sbyte)1)]
	[TestCase(new byte[] { 0xFF, 2, 3, 4 }, (sbyte)-1)]
	public void Int8Test(byte[] input, sbyte expected)
	{
		ReadOnlySpan<byte> bytes = input.ToArray().AsSpan();

		TestUtil(input, expected, bytes,
			1,
			Utils.GetInt8AndMove(ref bytes)
		);
	}
}
