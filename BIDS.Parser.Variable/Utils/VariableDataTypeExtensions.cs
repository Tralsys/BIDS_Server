using System;

namespace BIDS.Parser.Variable;

public static partial class Utils
{
	static public object? GetValueAndMoveNext(this VariableDataType type, ref ReadOnlySpan<byte> bytes)
		=> type switch
		{
			VariableDataType.Boolean => Utils.GetBooleanAndMove(ref bytes),

			VariableDataType.Int8 => Utils.GetInt8AndMove(ref bytes),
			VariableDataType.Int16 => Utils.GetInt16AndMove(ref bytes),
			VariableDataType.Int32 => Utils.GetInt32AndMove(ref bytes),
			VariableDataType.Int64 => Utils.GetInt64AndMove(ref bytes),

			VariableDataType.UInt8 => Utils.GetUInt8AndMove(ref bytes),
			VariableDataType.UInt16 => Utils.GetUInt16AndMove(ref bytes),
			VariableDataType.UInt32 => Utils.GetUInt32AndMove(ref bytes),
			VariableDataType.UInt64 => Utils.GetUInt64AndMove(ref bytes),

#if NET5_0_OR_GREATER
			VariableDataType.Float16 => Utils.GetFloat16AndMove(ref bytes),
#endif
			VariableDataType.Float32 => Utils.GetFloat32AndMove(ref bytes),
			VariableDataType.Float64 => Utils.GetFloat64AndMove(ref bytes),

			_ => null
		};

	static public byte[] GetBytes(this VariableDataType type, in object? obj)
		=> type switch
		{
			VariableDataType.Boolean => BitConverter.GetBytes((bool)(obj ?? false)),

			VariableDataType.Int8 => new byte[] { Convert.ToByte((sbyte)(obj ?? 0)) },
			VariableDataType.Int16 => BitConverter.GetBytes((short)(obj ?? 0)),
			VariableDataType.Int32 => BitConverter.GetBytes((int)(obj ?? 0)),
			VariableDataType.Int64 => BitConverter.GetBytes((long)(obj ?? 0)),

			VariableDataType.UInt8 => new byte[] { (byte)(obj ?? 0) },
			VariableDataType.UInt16 => BitConverter.GetBytes((ushort)(obj ?? 0)),
			VariableDataType.UInt32 => BitConverter.GetBytes((uint)(obj ?? 0)),
			VariableDataType.UInt64 => BitConverter.GetBytes((ulong)(obj ?? 0)),

#if NET5_0_OR_GREATER
			VariableDataType.Float16 => BitConverter.GetBytes((Half)(obj ?? default(Half))),
#endif
			VariableDataType.Float32 => BitConverter.GetBytes((float)(obj ?? 0)),
			VariableDataType.Float64 => BitConverter.GetBytes((double)(obj ?? 0)),

			_ => throw new NotSupportedException($"The ValueType `{type} ({(int)type})` is not supported")
		};
}
