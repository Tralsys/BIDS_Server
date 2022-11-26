using System;

namespace BIDS.Parser.Variable;

public static partial class Utils
{
	static public Array? GetSpecifiedTypeArray(this VariableDataType type, long length)
		=> type switch
		{
			VariableDataType.Boolean => new bool[length],

			VariableDataType.Int8 => new sbyte[length],
			VariableDataType.Int16 => new short[length],
			VariableDataType.Int32 => new int[length],
			VariableDataType.Int64 => new long[length],

			VariableDataType.UInt8 => new byte[length],
			VariableDataType.UInt16 => new ushort[length],
			VariableDataType.UInt32 => new uint[length],
			VariableDataType.UInt64 => new ulong[length],

#if NET5_0_OR_GREATER
			VariableDataType.Float16 => new Half[length],
#endif
			VariableDataType.Float32 => new float[length],
			VariableDataType.Float64 => new double[length],

			_ => null
		};
}
