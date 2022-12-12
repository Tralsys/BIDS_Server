using System.Runtime.InteropServices;

namespace BIDS.Parser;

public abstract record BIDSBinaryData_ArrayData(
	byte BiasNum,
	int[] DataArray
) : IBIDSBinaryData.IArrayData
{
	internal const int ARRAY_LENGTH = 128;
	internal const int CONTENT_LENGTH = ARRAY_LENGTH * sizeof(int);
	public int ContentLength => CONTENT_LENGTH;

	public abstract byte RawCommandType { get; }

	public byte RawDataType => BiasNum;

	public bool TryWriteToSpan(Span<byte> bytes)
	{
		if (bytes.Length < CONTENT_LENGTH)
			return false;

		for (int i = 0; i < DataArray.Length; i++)
		{
			BitConverter.GetBytes(DataArray[i])
				.CopyTo(bytes[(i * sizeof(int))..]);
		}

		return true;
	}
}

public record BIDSBinaryData_Panel : BIDSBinaryData_ArrayData
{
	public BIDSBinaryData_Panel(BIDSBinaryData_ArrayData original) : base(original)
	{
	}

	public BIDSBinaryData_Panel(byte BiasNum, int[] DataArray) : base(BiasNum, DataArray)
	{
	}

	public const byte RAW_COMMAND_TYPE = (byte)'p';
	public override byte RawCommandType { get; } = RAW_COMMAND_TYPE;

	public static IBIDSBinaryData From(ReadOnlySpan<byte> bytes, byte BiasNum)
	{
		if (bytes.Length < BIDSBinaryData_ArrayData.ARRAY_LENGTH)
			return new BIDSBinaryData_Error(BIDSBinaryDataErrorType.FewLength);

		int[] arr = new int[BIDSBinaryData_ArrayData.ARRAY_LENGTH];

		for (int i = 0; i < arr.Length; i++)
		{
			arr[i] = BitConverter.ToInt32(bytes[(i * sizeof(int))..]);
		}

		return new BIDSBinaryData_Panel(BiasNum, arr);
	}
}

public record BIDSBinaryData_Sound : BIDSBinaryData_ArrayData
{
	public BIDSBinaryData_Sound(BIDSBinaryData_ArrayData original) : base(original)
	{
	}

	public BIDSBinaryData_Sound(byte BiasNum, int[] DataArray) : base(BiasNum, DataArray)
	{
	}

	public const byte RAW_COMMAND_TYPE = (byte)'s';
	public override byte RawCommandType { get; } = RAW_COMMAND_TYPE;

	public static IBIDSBinaryData From(ReadOnlySpan<byte> bytes, byte BiasNum)
	{
		if (bytes.Length < BIDSBinaryData_ArrayData.ARRAY_LENGTH)
			return new BIDSBinaryData_Error(BIDSBinaryDataErrorType.FewLength);

		int[] arr = new int[BIDSBinaryData_ArrayData.ARRAY_LENGTH];

		for (int i = 0; i < arr.Length; i++)
		{
			arr[i] = BitConverter.ToInt32(bytes[(i * sizeof(int))..]);
		}

		return new BIDSBinaryData_Sound(BiasNum, arr);
	}
}
