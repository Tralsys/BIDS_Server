using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TR.BIDSsv
{
	/// <summary>便利関数をまとめたクラス</summary>
	static public class UFunc
	{
		/// <summary>両者が一致するならEmptyを, 一致しないなら新しいobjをToStringして返します.</summary>
		/// <param name="oldobj"></param>
		/// <param name="newobj"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static string Comp(in object oldobj, in object newobj) => Equals(oldobj, newobj) ? string.Empty : newobj.ToString();
		/// <summary>Byte Arrayに対し, BIDS規格のヘッダを記録します.</summary>
		/// <param name="ba">対象の配列</param>
		/// <param name="ba_2">コマンドタイプ</param>
		/// <param name="ba_3">データタイプ</param>
		/// <param name="index">配列の末尾記録</param>
		public static void SetBIDSHeader(this byte[] ba, in byte ba_2, in byte ba_3, ref int index)
		{
			ba[0] = (byte)ConstVals.BIN_CMD_HEADER_0;
			ba[1] = (byte)ConstVals.BIN_CMD_HEADER_1;
			ba[2] = ba_2;
			ba[3] = ba_3;
			index += 4;
		}

		//配列複製速度 参照 : http://dalmore.blog7.fc2.com/blog-entry-57.html

		#region ValueSet2Arr
		/// <summary>byte配列に引数の値を記録します.</summary>
		/// <param name="ba">書き込み先の配列</param>
		/// <param name="value">書き込む値</param>
		/// <param name="index">配列の末尾を記録した変数</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static void ValueSet2Arr(this byte[] ba, in int value, ref int index)
		{
			Buffer.BlockCopy(GetBytes(value), 0, ba, index, sizeof(int));
			index += sizeof(int);
		}
		/// <summary>byte配列に引数の値を記録します.</summary>
		/// <param name="ba">書き込み先の配列</param>
		/// <param name="value">書き込む値</param>
		/// <param name="index">配列の末尾を記録した変数</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static void ValueSet2Arr(this byte[] ba, in double value, ref int index)
		{
			Buffer.BlockCopy(GetBytes(value), 0, ba, index, sizeof(double));
			index += sizeof(double);
		}
		/// <summary>byte配列に引数の値を記録します.</summary>
		/// <param name="ba">書き込み先の配列</param>
		/// <param name="value">書き込む値</param>
		/// <param name="index">配列の末尾を記録した変数</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static void ValueSet2Arr(this byte[] ba, in float value, ref int index)
		{
			Buffer.BlockCopy(GetBytes(value), 0, ba, index, sizeof(float));
			index += sizeof(float);
		}
		/// <summary>byte配列に引数の値を記録します.</summary>
		/// <param name="ba">書き込み先の配列</param>
		/// <param name="value">書き込む値</param>
		/// <param name="index">配列の末尾を記録した変数</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static void ValueSet2Arr(this byte[] ba, in short value, ref int index)
		{
			Buffer.BlockCopy(GetBytes(value), 0, ba, index, sizeof(short));
			index += sizeof(short);
		}
		/// <summary>byte配列に引数の値を記録します.</summary>
		/// <param name="ba">書き込み先の配列</param>
		/// <param name="value">書き込む値</param>
		/// <param name="index">配列の末尾を記録した変数</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static void ValueSet2Arr(this byte[] ba, in bool value, ref int index)
		{
			Buffer.BlockCopy(GetBytes(value), 0, ba, index, sizeof(bool));
			index += sizeof(bool);
		}
		#endregion

		/// <summary>リトルエンディアンでないかどうかを返します(TRUEでBigEndian)</summary>
		private static readonly bool isLE = !BitConverter.IsLittleEndian;


		#region ToBytes
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in int arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in float arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in double arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in bool arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in uint arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in short arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in ushort arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in long arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in ulong arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		/// <summary>数値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する数値</param>
		/// <returns>変換後のbyte配列</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] ToBytes(this in char arg)
		{
			byte[] ba = BitConverter.GetBytes(arg);
			if (isLE) Array.Reverse(ba);
			return ba;
		}
		#endregion
		#region Get Value
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static int GetInt(this byte[] ab, in int ind = 0)
		{
			byte[] ba = new byte[sizeof(int)];
			Buffer.BlockCopy(ab, ind, ba, 0, ba.Length);
			if (isLE) Array.Reverse(ba);
			return BitConverter.ToInt32(ba, 0);
		}
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static float GetFloat(this byte[] ab, in int ind = 0)
		{
			byte[] ba = new byte[sizeof(float)];
			Buffer.BlockCopy(ab, ind, ba, 0, sizeof(float));
			if (isLE) Array.Reverse(ba);

			return BitConverter.ToSingle(ba, 0);
		}
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static double GetDouble(this byte[] ab, in int ind = 0)
		{
			byte[] ba = new byte[sizeof(double)];
			Buffer.BlockCopy(ab, ind, ba, 0, sizeof(double));
			if (isLE) Array.Reverse(ba);

			return BitConverter.ToDouble(ba, 0);
		}
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static bool GetBool(this byte[] ab, in int ind = 0)
		{
			byte[] ba = new byte[sizeof(bool)];
			Buffer.BlockCopy(ab, ind, ba, 0, sizeof(bool));
			if (isLE) Array.Reverse(ba);

			return BitConverter.ToBoolean(ba, 0);
		}
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static uint GetUInt(this byte[] ab, in int ind = 0)
		{
			byte[] ba = new byte[sizeof(uint)];
			Buffer.BlockCopy(ab, ind, ba, 0, sizeof(uint));
			if (isLE) Array.Reverse(ba);

			return BitConverter.ToUInt32(ba, 0);
		}
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static short GetShort(this byte[] ab, in int ind = 0)
		{
			byte[] ba = new byte[sizeof(short)];
			Buffer.BlockCopy(ab, ind, ba, 0, sizeof(short));
			if (isLE) Array.Reverse(ba);

			return BitConverter.ToInt16(ba, 0);
		}
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static ushort GetUShort(this byte[] ab, in int ind = 0)
		{
			byte[] ba = new byte[sizeof(ushort)];
			Buffer.BlockCopy(ab, ind, ba, 0, sizeof(ushort));
			if (isLE) Array.Reverse(ba);

			return BitConverter.ToUInt16(ba, 0);
		}
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static long GetLong(this byte[] ab, int ind = 0)
		{
			int sz = sizeof(long);

			byte[] ba = new byte[sz];
			int i = sz - ab.Length;
			Buffer.BlockCopy(ab, ind, ba, i < 0 ? 0 : i, sz);
			if (isLE) Array.Reverse(ba);

			return BitConverter.ToInt64(ba, 0);
		}
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static ulong GetULong(this byte[] ab, int ind = 0)
		{
			int sz = sizeof(long);

			byte[] ba = new byte[sz];
			int i = sz - ab.Length;
			Buffer.BlockCopy(ab, ind, ba, i < 0 ? 0 : i, sz);
			if (isLE) Array.Reverse(ba);

			return BitConverter.ToUInt64(ba, 0);
		}
		/// <summary>リトルエンディアンなbyte配列から値を取り出します.</summary>
		/// <param name="ab">ソースの配列</param>
		/// <param name="ind">値が入った位置</param>
		/// <returns>取得した値</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static char GetChar(this byte[] ab, int ind = 0)
		{
			int sz = sizeof(char);

			byte[] ba = new byte[sz];
			int i = sz - ab.Length;
			Buffer.BlockCopy(ab, ind, ba, i < 0 ? 0 : i, sz);
			if (isLE) Array.Reverse(ba);

			return BitConverter.ToChar(ba, 0);
		}
		#endregion
		#region GetBytes
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in int arg) => ToBytes(arg);
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in uint arg) => ToBytes(arg);
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in float arg) => ToBytes(arg);
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in double arg) => ToBytes(arg);
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in bool arg) => ToBytes(arg);
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in short arg) => ToBytes(arg);
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in ushort arg) => ToBytes(arg);
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in long arg) => ToBytes(arg);
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in ulong arg) => ToBytes(arg);
		/// <summary>指定の値をリトルエンディアンなByte配列に変換します.</summary>
		/// <param name="arg">変換する値</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static byte[] GetBytes(this in char arg) => ToBytes(arg);
		#endregion
		#region To Value
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static int ToInt32(this byte[] ab, int ind = 0) => GetInt(ab, ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static float ToSingle(this byte[] ab, int ind = 0) => GetFloat(ab, ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static double ToDouble(this byte[] ab, int ind = 0) => GetDouble(ab, ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static bool ToBoolean(this byte[] ab, int ind = 0) => GetBool(ab, ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static uint ToUInt32(this byte[] ab, int ind = 0) => GetUInt(ab, ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static short ToInt16(this byte[] ab, int ind = 0) => GetShort(ab, ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static ushort ToUInt16(this byte[] ab, int ind = 0) => GetUShort(ab, ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static long ToInt64(this byte[] ab, int ind = 0) => GetLong(ab, ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static ulong ToUInt64(this byte[] ab, int ind = 0) => GetULong(ab, ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static char ToChar(this byte[] ab, int ind = 0) => GetChar(ab, ind);

		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static int ToInt32(IEnumerable<byte> ab, int ind = 0) => GetInt(ab.ToArray(), ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static float ToSingle(IEnumerable<byte> ab, int ind = 0) => GetFloat(ab.ToArray(), ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static double ToDouble(IEnumerable<byte> ab, int ind = 0) => GetDouble(ab.ToArray(), ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static bool ToBoolean(IEnumerable<byte> ab, int ind = 0) => GetBool(ab.ToArray(), ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static uint ToUInt32(IEnumerable<byte> ab, int ind = 0) => GetUInt(ab.ToArray(), ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static short ToInt16(IEnumerable<byte> ab, int ind = 0) => GetShort(ab.ToArray(), ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static ushort ToUInt16(IEnumerable<byte> ab, int ind = 0) => GetUShort(ab.ToArray(), ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static long ToInt64(IEnumerable<byte> ab, int ind = 0) => GetLong(ab.ToArray(), ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static ulong ToUInt64(IEnumerable<byte> ab, int ind = 0) => GetULong(ab.ToArray(), ind);
		/// <summary>リトルエンディアンなbyte配列の指定位置から値を取り出します</summary>
		/// <param name="ab">byte配列</param>
		/// <param name="ind">取り出す値が格納された位置</param>
		/// <returns>変換結果</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static char ToChar(IEnumerable<byte> ab, int ind = 0) => GetChar(ab.ToArray(), ind);
		#endregion


		/// <summary>stringから整数値に変換(最初に見つかった数値文字から可能な限り変換を行う.)</summary>
		/// <param name="str">入力文字列</param>
		/// <returns>変換結果(数値が見つからなければnullを返す.)</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static int? String2INT(string str)
		{
			if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException();

			return (int?)String2Double(str);
		}

		static private readonly char[] NumAndSign = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '-', '.' };

		/// <summary>stringから小数値に変換(最初に見つかった数値文字から可能な限り変換を行う.)</summary>
		/// <param name="str">入力文字列</param>
		/// <returns>変換結果(数値が見つからなければnullを返す.)</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static double? String2Double(string str)
		{
			if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException();
			double v;
			//ref : https://stackoverflow.com/questions/2253653/where-can-i-find-a-net-implementation-of-atof
			int sInd = str.IndexOfAny(NumAndSign);
			if (sInd < 0) return null;
			return double.TryParse(
				new string(
					str.Substring(sInd).Trim()
					.TakeWhile((c) => (char.IsDigit(c) || c == '+' || c == '-' || c == '.'))
					.ToArray()), out v) ? (double?)v : null;
		}

		/// <summary>BIDS規格のString Commandを構成します.</summary>
		/// <param name="CMDType"></param>
		/// <param name="DType"></param>
		/// <param name="DNum"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static string BIDSCMDMaker(in char CMDType, in char DType, in int DNum)
			=> new StringBuilder(ConstVals.StringBuilder_Capacity).Append(ConstVals.CMD_HEADER).Append(CMDType).Append(DType).Append(DNum).ToString();
		/// <summary>BIDS規格のString Commandを構成します</summary>
		/// <param name="CMDType"></param>
		/// <param name="DType"></param>
		/// <param name="DNum"></param>
		/// <param name="data"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static string BIDSCMDMaker(in char CMDType, in char DType, in int DNum, in string data = null, in char separator = ConstVals.CMD_SEPARATOR)
		{
			StringBuilder sb = new StringBuilder(ConstVals.StringBuilder_Capacity).Append(ConstVals.CMD_HEADER).Append(CMDType).Append(DType).Append(DNum);
			if (data != null)
			{
				sb.Append(separator);
				sb.Append(data);
			}
			return sb.ToString();
		}
		/// <summary>BIDS規格のString Commandを構成します.</summary>
		/// <param name="CMDType"></param>
		/// <param name="DNum"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static string BIDSCMDMaker(in char CMDType, in int DNum)
			=> new StringBuilder(ConstVals.StringBuilder_Capacity).Append(ConstVals.CMD_HEADER).Append(CMDType).Append(DNum).ToString();
		/// <summary>BIDS規格のString Commandを構成します</summary>
		/// <param name="CMDType"></param>
		/// <param name="DNum"></param>
		/// <param name="data"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static string BIDSCMDMaker(in char CMDType, in int DNum, in string data, in char separator = ConstVals.CMD_SEPARATOR)
			=> new StringBuilder(ConstVals.StringBuilder_Capacity).Append(ConstVals.CMD_HEADER).Append(CMDType).Append(DNum).Append(separator).Append(data).ToString();

		/// <summary>State構造体のうち, 圧力状態値がすべて等しいか確認します.</summary>
		/// <param name="oldS"></param>
		/// <param name="newS"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static bool State_Pressure_IsSame(in State oldS, in State newS)
			=> oldS.BC == newS.BC
			&& oldS.BP == newS.BP
			&& oldS.ER == newS.ER
			&& oldS.MR == newS.MR
			&& oldS.SAP == newS.SAP;

#if !ID_SERCON
		/// <summary>配列が指定位置から指定数分だけ等しいか確認します.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ar1"></param>
		/// <param name="ar1ind"></param>
		/// <param name="ar2"></param>
		/// <param name="ar2ind"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static bool ArrayEqual<T>(T[] ar1, int ar1ind, T[] ar2, int ar2ind, in int len = -1)
		{
			int l = len;
			if (l <= 0) l = Math.Min((ar1?.Length ?? 0) - ar1ind, (ar2?.Length ?? 0) - ar2ind);
			if (!(ar1?.Length >= ar1ind + l && ar2?.Length >= ar2ind + l) || l <= 0) return false;
			byte IsEqual = 1;
			System.Threading.Tasks.Parallel.For(0, l, (i) =>
			{
				IsEqual &= (byte)(Equals(ar1[ar1ind + i], ar2[ar2ind + i]) ? 1 : 0);
			});
			return IsEqual == 1;
		}
		/// <summary>配列が指定位置から指定数分だけ等しいか確認します</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ar1"></param>
		/// <param name="ar2"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static bool ArrayEqual<T>(in T[] ar1, in T[] ar2, in int len = -1) => ArrayEqual<T>(ar1, 0, ar2, 0, len);
		/// <summary>配列が指定位置から指定数分だけ等しいか確認します</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ar"></param>
		/// <param name="item1ind"></param>
		/// <param name="item2ind"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public static bool ArrayEqual<T>(in this (T[], T[]) ar, in int item1ind = 0, in int item2ind = 0, in int len = -1) => ArrayEqual<T>(ar.Item1, item1ind, ar.Item2, item2ind, len);
#endif
	}
}
