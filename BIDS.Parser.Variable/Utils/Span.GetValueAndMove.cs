using System;
using System.Text;

namespace BIDS.Parser.Variable;

public static partial class Utils
{
	static public System.Boolean GetBooleanAndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..1];
		span = span[1..];

		return BitConverter.ToBoolean(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}

	#region Signed Integer
	static public System.SByte GetInt8AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = (sbyte)span[0];
		span = span[1..];
		return v;
	}

	static public System.Int16 GetInt16AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..2];
		span = span[2..];

		return BitConverter.ToInt16(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}

	static public System.Int32 GetInt32AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..4];
		span = span[4..];

		return BitConverter.ToInt32(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}

	static public System.Int64 GetInt64AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..8];
		span = span[8..];

		return BitConverter.ToInt64(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}

	// TODO: C#でInt128がサポートされ次第、Int128を使用した実装にする。
	static public System.Int64 GetInt128AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..16];
		span = span[16..];

		return BitConverter.ToInt64(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}
	#endregion

	#region Unsigned Integer
	static public System.Byte GetUInt8AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = span[0];
		span = span[1..];
		return v;
	}

	static public System.UInt16 GetUInt16AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..2];
		span = span[2..];

		return BitConverter.ToUInt16(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}

	static public System.UInt32 GetUInt32AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..4];
		span = span[4..];

		return BitConverter.ToUInt32(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}

	static public System.UInt64 GetUInt64AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..8];
		span = span[8..];

		return BitConverter.ToUInt64(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}

	// TODO: C#でUInt128がサポートされ次第、Int128を使用した実装にする。
	static public System.UInt64 GetUInt128AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..16];
		span = span[16..];

		return BitConverter.ToUInt64(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}
	#endregion

	#region Floating Point Number
#if NET5_0_OR_GREATER
	static public System.Half GetFloat16AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToHalf(span[0..2]);
		span = span[2..];
		return v;
	}
#endif

	static public float GetFloat32AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..4];
		span = span[4..];

		return BitConverter.ToSingle(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}

	static public double GetFloat64AndMove(ref ReadOnlySpan<byte> span)
	{
		ReadOnlySpan<byte> targetSpan = span[0..8];
		span = span[8..];

		return BitConverter.ToDouble(
			targetSpan
#if NETSTANDARD2_0
				.ToArray(),
			0
#endif
		);
	}
	#endregion

	static public string GetStringAndMove(ref ReadOnlySpan<byte> span)
	{
		int stringLength = 0;
		while (stringLength < span.Length && span[stringLength] != 0)
			stringLength++;

		string returnString = Encoding.UTF8.GetString(
			span[0..stringLength]
#if NETSTANDARD2_0
				.ToArray()
#endif
		);

		if (stringLength == span.Length)
			span = span[stringLength..];
		else
			span = span[(stringLength + 1)..];

		return returnString;
	}
}
