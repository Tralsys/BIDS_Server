using BIDS.Parser.VariableCommands;

namespace BIDS.Parser;

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

			VariableDataType.Float16 => Utils.GetFloat16AndMove(ref bytes),
			VariableDataType.Float32 => Utils.GetFloat32AndMove(ref bytes),
			VariableDataType.Float64 => Utils.GetFloat64AndMove(ref bytes),

			_ => null
		};
}
