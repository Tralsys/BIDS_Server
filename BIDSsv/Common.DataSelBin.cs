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
		static public void DataSelSend(IBIDSsv sv, in byte[] data, in Encoding enc) => sv?.Print(DataSelect(sv, data));

		/// <summary>Classify the data</summary>
		/// <param name="CName">Connection Instance</param>
		/// <param name="ba">Got Data</param>
		/// <returns>byte array to return, or array that calling program is needed to do something</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public byte[] DataSelect(IBIDSsv CName, in byte[] ba)
			=> ba?.Length >= 4 ? DataSelect(CName, ba) : null;//データ長4未満 or nullは対象外

		/// <summary>指定のデータ群から指定のデータを取り出します</summary>
		/// <param name="sv"></param>
		/// <param name="ba"></param>
		/// <param name="bsmd_in"></param>
		/// <param name="od_in"></param>
		/// <param name="pa_in"></param>
		/// <param name="sa_in"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public byte[] DataSelect(in IBIDSsv SVc, byte[] ba, in BIDSSharedMemoryData? bsmd_in = null, in OpenD? od_in = null, in int[] pa_in = null, in int[] sa_in = null)
		{
			if (!(ba?.Length >= ConstVals.CMD_LEN_MIN)) return null;

			if (ba[ConstVals.BINARY_DATA_POS.HEADER_0] == ConstVals.BINARY_DATA.HEADER_0 && ba[1] == ConstVals.BIN_CMD_HEADER_1)//Binaryデータ
				return DataSelBin(ba, bsmd_in, od_in, pa_in, sa_in);
			else if (ba[0] == ConstVals.STR_CMD_HEADER_0)//文字列データ
				return DataSelBin_StrCMD(SVc, ba, bsmd_in, od_in, pa_in, sa_in);
			else return null;
		}

		#region Binaryコマンドの処理メソッド群
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static private byte[] DataSelBin_TypIsInfo(in byte[] ba, in BIDSSharedMemoryData? bsmd_in = null, in OpenD? od_in = null, in int[] pa_in = null, in int[] sa_in = null)
		{
			switch ((ConstVals.BIN_CMD_INFOD_TYPES)ba[3])
			{
				case ConstVals.BIN_CMD_INFOD_TYPES.SPEC://Spec
					//if (ba.GetShort(4) < sv.Version) return null;//バージョンチェックは一時無効化
					/*else*/ if (ba.Length >= AutoSendSetting.BasicConstSize)
					{
						int i = 0;
						BIDSSharedMemoryData bsmd_ = bsmd_in ?? SMemLib.BIDSSMemData;
						Spec s = bsmd_.SpecData;
						s.B = ba.GetShort(i += 8);
						s.P = ba.GetShort(i += 2);
						s.A = ba.GetShort(i += 2);
						s.J = ba.GetShort(i += 2);
						s.C = ba.GetShort(i += 2);
						bsmd_.SpecData = s;
						SMemLib.Write(bsmd_);

						OpenD od = od_in ?? SMemLib.OpenData;
						od.SelfBCount = ba.GetShort(i++);
						SMemLib.Write(od);
					}
					//else return ba;
					return null;

				case ConstVals.BIN_CMD_INFOD_TYPES.STATE://State
					if (ba.Length >= AutoSendSetting.BasicCommonSize)
					{
						BIDSSharedMemoryData bsmd = bsmd_in ?? SMemLib.BIDSSMemData;
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
						SMemLib.Write(bsmd);
					}
					//else return ba;
					return null;

				case ConstVals.BIN_CMD_INFOD_TYPES.BVE5D://BVE5D
					break;
				case ConstVals.BIN_CMD_INFOD_TYPES.OPEND://OpenD
					break;
				case ConstVals.BIN_CMD_INFOD_TYPES.HANDLE://Handle
					if (ba.Length >= AutoSendSetting.BasicHandleSize)
					{
						BIDSSharedMemoryData bsmd = bsmd_in ?? SMemLib.BIDSSMemData;
						OpenD od = od_in ?? SMemLib.OpenData;
						Hand h = bsmd.HandleData;
						int i = 0;
						h.P = ba.GetInt(i += 4);
						h.B = ba.GetInt(i += 4);
						h.R = ba.GetInt(i += 4);
						od.SelfBPosition = ba.GetInt(i += 4);

						bsmd.HandleData = h;
						SMemLib.Write(bsmd);

						SMemLib.Write(od);
					}
					return null;
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static private byte[] DataSelBin(in byte[] ba, in BIDSSharedMemoryData? bsmd_in = null, in OpenD? od_in = null, in int[] pa_in = null, in int[] sa_in = null)
		{
			if (!(ba?.Length >= 6)) return null;//データ長6未満 or nullは対象外(念のためチェック)

			//Header Check
			if (ba[0] != ConstVals.BIN_CMD_HEADER_0 || ba[1] != ConstVals.BIN_CMD_HEADER_1) return null;//t:0x74, r:0x72

			if (ba[2] == ConstVals.BIN_CMD_INFO_DATA)
				return DataSelBin_TypIsInfo(ba, bsmd_in, od_in, pa_in, sa_in);
			else if (ba[2] == ConstVals.BIN_CMD_PANEL_DATA)
				return DataSelBin_TypeIsArray(ba, pa_in ?? SMemLib.PanelA, ConstVals.PANEL_BIN_ARR_PRINT_COUNT, (i) => SMemLib.WritePanel(i));
			else if (ba[2] == ConstVals.BIN_CMD_SOUND_DATA)
				return DataSelBin_TypeIsArray(ba, sa_in ?? SMemLib.SoundA, ConstVals.SOUND_BIN_ARR_PRINT_COUNT, (i) => SMemLib.WriteSound(i));

			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static private byte[] DataSelBin_TypeIsArray(byte[] ba, int[] DataArray, in int ArrStdLen, Action<int[]> WriteTo)
		{
			if (ba.Length < (ArrStdLen * sizeof(int) + 4))//配列データ量, ヘッダ
				return null;
			try
			{
				int[] cache = DataArray;
				int ind = ba[3],
					bias = ind * ArrStdLen,
					needed_len = bias + ArrStdLen;

				if (needed_len > cache.Length)//入力データがキャッシュよりも長い
				{
					int[] n_cache = new int[bias + ArrStdLen];
					Buffer.BlockCopy(cache, 0, n_cache, 0, cache.Length * sizeof(int));
					cache = n_cache;//参照の更新
				}

				Parallel.For(0, ArrStdLen, (i) => cache[bias + i] = ba.GetInt(4 + (4 * i)));
				
				WriteTo?.Invoke(cache);
			}
			catch (ObjectDisposedException) { return null; }
			catch (Exception) { throw; }

			return null;
		}
		#endregion

		#region String形式コマンドの処理メソッド群
		static private (char, int) GetDTypDNum(in byte[] ba) => ((char)ba[ConstVals.DTYPE_CHAR_POS], GetIntValueFromBA(ba, ConstVals.DNUM_POS));
		
		static private int GetIntValueFromBA(in byte[] ba, in int pos) { int v; return GetIntValueFromBA(ba, pos, out v); }
		static private int GetIntValueFromBA(in byte[] ba, in int pos, out int len)
		{
			int value = 0;
			bool IsMinus = ba[pos] == '-';//符号チェック

			for (len = IsMinus ? 1 : 0; (len + ConstVals.DNUM_POS) < ba.Length && char.IsDigit((char)ba[ConstVals.DNUM_POS + len]); len++)
				value = value * 10 + (ba[ConstVals.DNUM_POS + len] - '0');

			len -= 1;

			if (IsMinus)
				value *= -1;

			return value;
		}

		static readonly byte[] BA_X0 = new byte[] { (byte)'X', (byte)'0' };

		static private byte[] DataSelBin_StrCMD(in IBIDSsv SVc, in byte[] ba, in BIDSSharedMemoryData? bsmd_in = null, in OpenD? od_in = null, in int[] pa_in = null, in int[] sa_in = null)
		{
			if (ba is null || ba.Length < 4) return null;//NULL/Lengthチェック(念のため)

			if (ba[0] != ConstVals.STR_CMD_HEADER_0 || ba[1] != ConstVals.STR_CMD_HEADER_1) return null;//ヘッダチェック(念のため)

			char dtyp;
			int dnum = 0;
			List<byte> retarr = new List<byte>(ba);
			bool IsBAReply = false;

			for (int i = 0; i < ba.Length; i++)
				IsBAReply |= ba[i] == (byte)ConstVals.CMD_SEPARATOR;

			switch((char)ba[2])//Command Type
			{
				case ConstVals.CMD_AUTOSEND_DEL:
				case ConstVals.CMD_AUTOSEND_ADD:
					(dtyp, dnum) = GetDTypDNum(ba);

					if (IsBAReply)
					{
						DataGot(ba, dtyp, dnum);
						return null;
					}

					try
					{
						retarr.AddRange(DataSelBin_StrCMD_ASAddRmv(SVc, dtyp, dnum, (char)ba[2] == ConstVals.CMD_AUTOSEND_ADD));
					}
					catch (Exception)
					{
						return Encoding.Default.GetBytes(Errors.GetCMD(Errors.ErrorNums.UnknownError));
					}

					return retarr.ToArray();

				case ConstVals.CMD_INFOREQ:
					(dtyp, dnum) = GetDTypDNum(ba);

					if (IsBAReply)
					{
						DataGot(ba, dtyp, dnum);
						return null;
					}

					retarr.AddRange(ToBinArr(DataPicker(dtyp, dnum)));

					return retarr.ToArray();

				case ConstVals.CMD_BREAK:
				case ConstVals.CMD_POWER:
				case ConstVals.CMD_SPoleMC:
					if (IsBAReply)
						return null;

					int retval = GetIntValueFromBA(ba, ConstVals.DTYPE_CHAR_POS);
					Hands hs = CtrlInput.GetHandD();
					switch((char)ba[2])
					{
						case ConstVals.CMD_BREAK:
							hs.B = retval;
							break;
						case ConstVals.CMD_POWER:
							hs.P = retval;
							break;
						case ConstVals.CMD_SPoleMC:
							hs.S = retval;
							break;
					}

					CtrlInput.SetHandD(ref hs);
					retarr.AddRange(BA_X0);
					return retarr.ToArray();

				case ConstVals.CMD_REVERSER:
					Hands hsr = CtrlInput.GetHandD();
					int rretval = ba[ConstVals.DTYPE_CHAR_POS] switch
					{
						(byte)'F' => 1,
						(byte)'N' => 0,
						(byte)'R' => -1,
						(byte)'B' => -1,
						_ => GetIntValueFromBA(ba, ConstVals.DTYPE_CHAR_POS)
					};
					hsr.R = rretval;
					CtrlInput.SetHandD(ref hsr);
					retarr.AddRange(BA_X0);
					return retarr.ToArray();
				case ConstVals.CMD_KeyCtrl:
					if (IsBAReply)
						return null;

					int knum = GetIntValueFromBA(ba, ConstVals.DTYPE_CHAR_POS);
					if (knum < 0 || CtrlInput.KeyArrSizeMax <= knum)
						return null;

					bool? IsPushed = (char)ba[ba.Length - 1] switch { 'P' => true, 'R' => false, _ => null };

					if (IsPushed is not null)
						CtrlInput.SetIsKeyPushed(knum, IsPushed ?? false);
					else
						return null;

					retarr.AddRange(BA_X0);
					return retarr.ToArray();
			}

			return null;
		}

		static private byte[] DataSelBin_StrCMD_ASAddRmv(in IBIDSsv SVc, in char dtyp, in int dnum, in bool IsAddCMD)
		{
			object obj = (int)0;
			ASList asl;
			switch (dtyp)
			{
				case ConstVals.DTYPE_PANEL:
				case ConstVals.DTYPE_PANEL_ARR:
					PDAutoList ??= new ASList();
					asl = PDAutoList;
					break;
				case ConstVals.DTYPE_SOUND:
				case ConstVals.DTYPE_SOUND_ARR:
					SDAutoList ??= new ASList();
					asl = SDAutoList;
					break;
				default:
					AutoNumL ??= new ASList();
					asl = AutoNumL;
					break;
			}

			bool Containing = asl.Contains(SVc, dnum, dtyp);
			if (IsAddCMD)
			{
					obj = DataPicker(dtyp, dnum);//初期値の取得
					if (!Containing) asl.Add(SVc, dnum, dtyp);
			}
			else if (Containing) asl.Remove(SVc, dnum, dtyp);

			return ToBinArr(obj);
		}

		static private byte[] ToBinArr(object o)
		{
			List<byte> ba = new List<byte>();
			if (o is Array a)
				foreach (object v in a)
				{
					ba.Add((byte)ConstVals.CMD_SEPARATOR);
					ba.AddRange(Encoding.Default.GetBytes(v.ToString()));
				}

			else
			{
				ba.Add((byte)ConstVals.CMD_SEPARATOR);
				ba.AddRange(Encoding.Default.GetBytes(o.ToString()));
			}

			return ba.ToArray();
		}

		#endregion
	}
}
