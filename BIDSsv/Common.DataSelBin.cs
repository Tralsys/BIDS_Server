using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
	static public partial class Common
	{
		/* byte配列を扱うDataSel Methodを実装 */

		/// <summary>必要なデータを取得すると同時に, 送信まで行う.</summary>
		/// <param name="sv">通信インスタンス</param>
		/// <param name="data">入力データ</param>
		/// <param name="enc">使用するエンコーディング</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public void DataSelSend(IBIDSsv sv, in byte[] data, in Encoding enc) => sv?.Print(DataSelect(sv, data, enc));

		/// <summary>Classify the data</summary>
		/// <param name="CName">Connection Instance</param>
		/// <param name="ba">Got Data</param>
		/// <param name="enc">Encording</param>
		/// <returns>byte array to return, or array that calling program is needed to do something</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public byte[] DataSelect(IBIDSsv CName, in byte[] ba, in Encoding enc)
		{
			if (!(ba?.Length >= 4)) return null;//データ長4未満 or nullは対象外

			//StringデータかBinaryデータかを識別し, 適切なメソッドに処理を渡す

			if (ba[0] == (byte)'T') return enc.GetBytes(DataSelect(CName, enc.GetString(ba)) ?? string.Empty);//It is string data
			else return DataSelBin(ba);
		}

		/// <summary>指定のデータ群から指定のデータを取り出します</summary>
		/// <param name="sv"></param>
		/// <param name="ba"></param>
		/// <param name="bsmd_in"></param>
		/// <param name="od_in"></param>
		/// <param name="pa_in"></param>
		/// <param name="sa_in"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public byte[] DataSelect(IBIDSsv sv, in byte[] ba, in BIDSSharedMemoryData? bsmd_in = null, in OpenD? od_in = null, in int[] pa_in = null, in int[] sa_in = null)
		{
			
			DataSelect(sv, ba, null, null, null, null);
			if (!(ba?.Length >= ConstVals.CMD_LEN_MIN)) return null;
			if (ba[ConstVals.BINARY_DATA_POS.HEADER_0] == ConstVals.BINARY_DATA.HEADER_0)//Binaryデータ
			{
				if (ba[ConstVals.BINARY_DATA_POS.HEADER_1] != ConstVals.BINARY_DATA.HEADER_1) return null;//ここで弾かれたらBIDSのデータじゃない

				switch (ba[ConstVals.BINARY_DATA_POS.TYP])
				{
					case ConstVals.BINARY_DATA.TYP_SECSYS_DATA://未実装
						break;
					case ConstVals.BINARY_DATA.TYP_INFO_DATA:
						switch ((ConstVals.BIN_CMD_INFOD_TYPES)ba[3])
						{
							case ConstVals.BIN_CMD_INFOD_TYPES.SPEC://Spec
								if (ba.GetShort(4) < sv.Version) return null;
								else if (ba.Length >= AutoSendSetting.BasicConstSize)
								{
									int i = 0;
									Spec s = BSMD.SpecData;
									s.B = ba.GetShort(i += 8);
									s.P = ba.GetShort(i += 2);
									s.A = ba.GetShort(i += 2);
									s.J = ba.GetShort(i += 2);
									s.C = ba.GetShort(i += 2);
									OpenD od = OD;
									od.SelfBCount = ba.GetShort(i++);
									OD = od;
									BIDSSharedMemoryData bsmd_ = BSMD;
									bsmd_.SpecData = s;
								}
								//else return ba;
								break;
							case ConstVals.BIN_CMD_INFOD_TYPES.STATE://State
								if (ba.Length >= AutoSendSetting.BasicCommonSize)
								{
									BIDSSharedMemoryData bsmd = BSMD;
									State s = bsmd.StateData;
									int i = 0;
									s.Z = ba.GetDouble(i += 4);
									s.V = ba.GetFloat(i += 8);
									s.I = ba.GetFloat(i += 4);
									//_ = ba.GetFloat(i += 4);//WireVoltage
									s.T = ba.GetInt(i += 8);
									s.BC = ba.GetFloat(i += 4);
									s.MR = ba.GetFloat(i += 4);
									s.ER = ba.GetFloat(i += 4);
									s.BP = ba.GetFloat(i += 4);
									s.SAP = ba.GetFloat(i += 4);
									bsmd.IsDoorClosed = ba[i += 8] == 1;
									bsmd.StateData = s;
									BSMD = bsmd;
								}
								//else return ba;
								break;
							case ConstVals.BIN_CMD_INFOD_TYPES.BVE5D://BVE5D
								break;
							case ConstVals.BIN_CMD_INFOD_TYPES.OPEND://OpenD
								break;
							case ConstVals.BIN_CMD_INFOD_TYPES.HANDLE://Handle
								if (ba.Length >= AutoSendSetting.BasicHandleSize)
								{
									BIDSSharedMemoryData bsmd = BSMD;
									OpenD od = OD;
									Hand h = bsmd.HandleData;
									int i = 0;
									h.P = ba.GetInt(i += 4);
									h.B = ba.GetInt(i += 4);
									h.R = ba.GetInt(i += 4);
									od.SelfBPosition = ba.GetInt(i += 4);
									bsmd.HandleData = h;
									BSMD = bsmd;
									OD = od;
								}
								break;
						}
						break;
					case ConstVals.BINARY_DATA.TYP_HANDLE_CTRL:
						break;
					case ConstVals.BINARY_DATA.TYP_PANEL_DATA:

						break;
					case ConstVals.BINARY_DATA.TYP_REQUEST://未実装
						break;
					case ConstVals.BINARY_DATA.TYP_SOUND_DATA:
						break;
				}
			}
			else if (ba[0] == ConstVals.STR_CMD_HEADER_0)//文字列データ
			{

			}
			else return null;

			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static private byte[] DataSelBin(byte[] ba)
		{
			if (!(ba?.Length >= 6)) return null;//データ長6未満 or nullは対象外(念のためチェック)

			//Header Check
			if (ba[0] != ConstVals.BIN_CMD_HEADER_0 || ba[1] != ConstVals.BIN_CMD_HEADER_1) return null;//t:0x74, r:0x72
			if (ba[2] == ConstVals.BIN_CMD_INFO_DATA)
			{
				switch ((ConstVals.BIN_CMD_INFOD_TYPES)ba[3])
				{
					case ConstVals.BIN_CMD_INFOD_TYPES.SPEC://Spec
						if (ba.GetShort(4) < Version) return null;
						else if (ba.Length >= AutoSendSetting.BasicConstSize)
						{
							BIDSSharedMemoryData bsmd = BSMD;
							OpenD od = OD;
							Spec s = bsmd.SpecData;
							int i = 0;
							s.B = ba.GetShort(i += 8);
							s.P = ba.GetShort(i += 2);
							s.A = ba.GetShort(i += 2);
							s.J = ba.GetShort(i += 2);
							s.C = ba.GetShort(i += 2);
							od.SelfBCount = ba.GetShort(i++);
							bsmd.SpecData = s;
							BSMD = bsmd;
							OD = od;
						}
						//else return ba;
						break;
					case ConstVals.BIN_CMD_INFOD_TYPES.STATE://State
						if (ba.Length >= AutoSendSetting.BasicCommonSize)
						{
							BIDSSharedMemoryData bsmd = BSMD;
							State s = bsmd.StateData;
							int i = 0;
							s.Z = ba.GetDouble(i += 4);
							s.V = ba.GetFloat(i += 8);
							s.I = ba.GetFloat(i += 4);
							//_ = ba.GetFloat(i += 4);//WireVoltage
							s.T = ba.GetInt(i += 8);
							s.BC = ba.GetFloat(i += 4);
							s.MR = ba.GetFloat(i += 4);
							s.ER = ba.GetFloat(i += 4);
							s.BP = ba.GetFloat(i += 4);
							s.SAP = ba.GetFloat(i += 4);
							bsmd.IsDoorClosed = ba[i += 8] == 1;
							bsmd.StateData = s;
							BSMD = bsmd;
						}
						//else return ba;
						break;
					case ConstVals.BIN_CMD_INFOD_TYPES.BVE5D://BVE5D
						break;
					case ConstVals.BIN_CMD_INFOD_TYPES.OPEND://OpenD
						break;
					case ConstVals.BIN_CMD_INFOD_TYPES.HANDLE://Handle
						if (ba.Length >= AutoSendSetting.BasicHandleSize)
						{
							BIDSSharedMemoryData bsmd = BSMD;
							OpenD od = OD;
							Hand h = bsmd.HandleData;
							int i = 0;
							h.P = ba.GetInt(i += 4);
							h.B = ba.GetInt(i += 4);
							h.R = ba.GetInt(i += 4);
							od.SelfBPosition = ba.GetInt(i += 4);
							bsmd.HandleData = h;
							BSMD = bsmd;
							OD = od;
						}
						break;
				}
			}
			else if (ba[2] == ConstVals.BIN_CMD_PANEL_DATA)
			{
				if (ba.Length < (ConstVals.PANEL_BIN_ARR_PRINT_COUNT * sizeof(int) + sizeof(int)) * 4) return null;
				try
				{
					int[] cache = SMemLib.PanelA;
					int ind = ba[3], bias = ind * ConstVals.PANEL_BIN_ARR_PRINT_COUNT, needed_len = bias + ConstVals.PANEL_BIN_ARR_PRINT_COUNT;
					if (needed_len > cache.Length)//入力データがキャッシュよりも長い
					{
						int[] n_cache = new int[bias + ConstVals.PANEL_BIN_ARR_PRINT_COUNT];
						Buffer.BlockCopy(cache, 0, n_cache, 0, cache.Length * sizeof(int));
						cache = n_cache;//参照の更新
					}
					Parallel.For(0, ConstVals.PANEL_BIN_ARR_PRINT_COUNT, (i) => cache[bias + i] = ba.GetInt(4 + (4 * i)));
					PDA = cache;
				}
				catch (ObjectDisposedException) { return null; }
				catch (Exception) { throw; }
			}
			else if (ba[2] == ConstVals.BIN_CMD_SOUND_DATA)
			{
				if (ba.Length < (ConstVals.SOUND_BIN_ARR_PRINT_COUNT * sizeof(int) + sizeof(int)) * 4) return null;
				try
				{
					int[] cache = SMemLib.SoundA;
					int ind = ba[3], bias = ind * ConstVals.SOUND_BIN_ARR_PRINT_COUNT, needed_len = bias + ConstVals.SOUND_BIN_ARR_PRINT_COUNT;
					if (needed_len > cache.Length)//入力データがキャッシュよりも長い
					{
						int[] n_cache = new int[bias + ConstVals.SOUND_BIN_ARR_PRINT_COUNT];
						Buffer.BlockCopy(cache, 0, n_cache, 0, cache.Length * sizeof(int));
						cache = n_cache;//参照の更新
					}
					Parallel.For(0, ConstVals.SOUND_BIN_ARR_PRINT_COUNT, (i) => cache[bias + i] = ba.GetInt(4 + (4 * i)));
					SDA = cache;
				}
				catch (ObjectDisposedException) { return null; }
				catch (Exception) { throw; }
			}

			return null;
		}
	}
}
