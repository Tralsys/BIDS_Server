using System;
using System.Text;

namespace BIDS.Parser.Variable;

public static partial class Utils
{
	static public System.Boolean GetBooleanAndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToBoolean(span[..1]);
		span = span[1..];
		return v;
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
		var v = BitConverter.ToInt16(span[..2]);
		span = span[2..];
		return v;
	}

	static public System.Int32 GetInt32AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToInt32(span[..4]);
		span = span[4..];
		return v;
	}

	static public System.Int64 GetInt64AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToInt64(span[..8]);
		span = span[8..];
		return v;
	}

	// TODO: C#でInt128がサポートされ次第、Int128を使用した実装にする。
	static public System.Int64 GetInt128AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToInt64(span[..16]);
		span = span[16..];
		return v;
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
		var v = BitConverter.ToUInt16(span[..2]);
		span = span[2..];
		return v;
	}

	static public System.UInt32 GetUInt32AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToUInt32(span[..4]);
		span = span[4..];
		return v;
	}

	static public System.UInt64 GetUInt64AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToUInt64(span[..8]);
		span = span[8..];
		return v;
	}

	// TODO: C#でUInt128がサポートされ次第、Int128を使用した実装にする。
	static public System.UInt64 GetUInt128AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToUInt64(span[..16]);
		span = span[16..];
		return v;
	}
	#endregion

	#region Floating Point Number
#if NET5_0_OR_GREATER
	static public System.Half GetFloat16AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToHalf(span[..2]);
		span = span[2..];
		return v;
	}
#endif

	static public float GetFloat32AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToSingle(span[..4]);
		span = span[4..];
		return v;
	}

	static public double GetFloat64AndMove(ref ReadOnlySpan<byte> span)
	{
		var v = BitConverter.ToDouble(span[..8]);
		span = span[8..];
		return v;
	}
	#endregion

	static public string GetStringAndMove(ref ReadOnlySpan<byte> span)
	{
		int stringLength = 0;
		while (stringLength < span.Length && span[stringLength] != 0)
			stringLength++;

		string returnString = Encoding.UTF8.GetString(span[..stringLength]);

		if (stringLength == span.Length)
			span = span[stringLength..];
		else
			span = span[(stringLength + 1)..];

		return returnString;
	}
}
