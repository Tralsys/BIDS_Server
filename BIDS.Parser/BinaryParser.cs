using BIDS.Parser.Variable;

namespace BIDS.Parser;

public partial class BinaryParser : IBinaryParser
{
	const int HEADER_LEN = 4;

	internal const byte INFO_CMD_TYPE_VALUE = (byte)'b';
	internal const byte VARIABLE_CMD_TYPE_VALUE = (byte)'v';

	public VariableCmdParser VariableCmdParser { get; }

	public BinaryParser() : this(new Dictionary<int, VariableStructure>()) { }

	public BinaryParser(IReadOnlyDictionary<int, VariableStructure> dataTypeDict)
	{
		VariableCmdParser = new(dataTypeDict);
	}

	public IBIDSBinaryData From(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length < HEADER_LEN)
			return new BIDSBinaryData_Error(BIDSBinaryDataErrorType.FewLength);

		if (BitConverter.ToInt16(bytes[0..]) == IBIDSBinaryData.RAW_DATA_IDENTIFIER)
		{
			ReadOnlySpan<byte> contents = bytes[HEADER_LEN..];
			byte CommandType = bytes[2];
			byte DataType = bytes[3];

			return CommandType switch
			{
				BIDSBinaryData_Error.RAW_COMMAND_TYPE => BIDSBinaryData_Error.From(contents),

				BIDSBinaryData_Panel.RAW_COMMAND_TYPE => BIDSBinaryData_Panel.From(contents, DataType),
				BIDSBinaryData_Sound.RAW_COMMAND_TYPE => BIDSBinaryData_Sound.From(contents, DataType),

				INFO_CMD_TYPE_VALUE => DataType switch
				{
					BIDSBinaryData_Handle.RAW_DATA_TYPE => BIDSBinaryData_Handle.From(contents),
					BIDSBinaryData_Spec.RAW_DATA_TYPE => BIDSBinaryData_Spec.From(contents),
					BIDSBinaryData_State.RAW_DATA_TYPE => BIDSBinaryData_State.From(contents),

					_ => new BIDSBinaryData_Error(BIDSBinaryDataErrorType.UnknownDataType)
				},

				// Variable Command系の処理用関数を別途実装して、それを呼ぶ
				VARIABLE_CMD_TYPE_VALUE => OnVariableCmdGot(contents),

				_ => new BIDSBinaryData_Error(BIDSBinaryDataErrorType.UnknownCommandType)
			};
		}
		else
		{
			return new BIDSBinaryData_Error(BIDSBinaryDataErrorType.NotBIDSCommand);
		}
	}
}
