using System;
using System.Collections.Generic;
using System.Linq;

namespace BIDS.Parser.Variable;

public record VariableStructure(int DataTypeId, IReadOnlyList<VariableStructure.IDataRecord> Records) : IVariableCmdResult
{
	public interface IDataRecord
	{
		VariableDataType Type { get; }

		string Name { get; }

		IDataRecord With(ref ReadOnlySpan<byte> bytes);

		IEnumerable<byte> GetStructureBytes();

		IEnumerable<byte> GetBytes();
	}

	public record DataRecord(VariableDataType Type, string Name, object? Value = null) : IDataRecord
	{
		public IDataRecord With(ref ReadOnlySpan<byte> bytes)
		{
			return this with
			{
				Value = this.Type.GetValueAndMoveNext(ref bytes)
			};
		}

		public IEnumerable<byte> GetStructureBytes()
			=> BitConverter.GetBytes((int)this.Type).Concat(System.Text.Encoding.UTF8.GetBytes(this.Name)).Append((byte)0);

		public IEnumerable<byte> GetBytes()
			=> this.Type.GetBytes(this.Value);
	}

	public record ArrayStructure(VariableDataType ElemType, string Name, object?[]? ValueArray = null) : IDataRecord
	{
		VariableDataType IDataRecord.Type => VariableDataType.Array;

		public IDataRecord With(ref ReadOnlySpan<byte> bytes)
		{
			int arrayLength = Utils.GetInt32AndMove(ref bytes);

			object?[] array = new object?[arrayLength];
			for (int i = 0; i < arrayLength; i++)
				array[i] = this.ElemType.GetValueAndMoveNext(ref bytes);

			return this with
			{
				ValueArray = array
			};
		}

		public IEnumerable<byte> GetStructureBytes()
			=> BitConverter.GetBytes((int)VariableDataType.Array)
					.Concat(BitConverter.GetBytes((int)this.ElemType))
					.Concat(System.Text.Encoding.UTF8.GetBytes(this.Name)).Append((byte)0);

		public IEnumerable<byte> GetBytes()
		{
			IEnumerable<byte> arr = BitConverter.GetBytes(this.ValueArray?.Length ?? 0);

			if (this.ValueArray is not null)
				foreach (var v in this.ValueArray)
					arr = arr.Concat(this.ElemType.GetBytes(v));

			return arr;
		}
	}

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
