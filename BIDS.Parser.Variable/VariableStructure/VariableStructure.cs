using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BIDS.Parser.Variable;

public partial record VariableStructure(int DataTypeId, string Name, IReadOnlyList<VariableStructure.IDataRecord> Records) : IVariableCmdResult
{
	/// <summary>この構造を用いて、指定のバイト配列を解析する</summary>
	/// <param name="bytes">受け取ったデータ</param>
	/// <returns>解析結果</returns>
	public VariableStructurePayload With(ReadOnlySpan<byte> bytes)
	{
		VariableStructurePayload payload = new(this.DataTypeId);

		foreach (var recordInfo in this.Records)
		{
			IDataRecord data = recordInfo.With(ref bytes);

			payload.Add(recordInfo.Name, data);
		}

		return payload;
	}

	public IEnumerable<byte> GetStructureBytes()
	{
		IEnumerable<byte> bytes = BitConverter.GetBytes(this.DataTypeId);

		byte[] nameBytes = Encoding.UTF8.GetBytes(Name.Trim('\0'));
		bytes = bytes
			.Concat(nameBytes)
			.Concat(new byte[] { 0 });

		foreach (var v in this.Records)
			bytes = bytes.Concat(v.GetStructureBytes());

		return bytes;
	}

	public IEnumerable<byte> GetBytes()
	{
		IEnumerable<byte> bytes = BitConverter.GetBytes(this.DataTypeId);

		foreach (var v in this.Records)
			bytes = bytes.Concat(v.GetBytes());

		return bytes;
	}
};
