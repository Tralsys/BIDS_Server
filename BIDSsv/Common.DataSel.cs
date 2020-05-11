using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
  static public partial class Common
  {
    static readonly byte[] ZeroX4 = new byte[]{ 0, 0, 0, 0 };

		#region String Format
		static readonly string StrFormat_BSMD_SpecAll = null;
    static readonly string StrFormat_BSMD_StateAll = null;
    static readonly string StrFormat_BSMD_Pressures = null;
    static readonly string StrFormat_BSMD_Time = null;//"{0}:{1}:{2}.{3}"
    static readonly string StrFormat_BSMD_HandAll = null;
    #endregion

    /// <summary>ヘッダがTOであるコマンドでの要求を処理する.</summary>
    /// <param name="GotStr">接尾辞が取り除かれたコマンド文字列</param>
    /// <returns>返信すべき文字列(なければnull)</returns>
    static private string DataSelTO(in string GotStr)
    {
      if (string.IsNullOrWhiteSpace(GotStr)) return null;
      char[] GotChars = GotStr.ToCharArray();
      switch (GotChars[2])
      {
        case 'R':
          int? revVbuf = GotChars[3] switch
          {
            'F' => 1,
            'N' => 0,
            'R' => -1,
            'B' => -1,
            _ => null
          };
          if (revVbuf != null) ReverserNum = revVbuf ?? 0;
          break;
        case 'K':
          int? KNum = UFunc.String2INT(GotStr.Substring(3));
          if (KNum == null) break;//値なし
          int[] ka = GIPI.GetBtJobNum(KNum ?? 0);
          if (!(ka?.Length > 0)) break;//機能該当なし
          int i = 0;
          switch (GotChars.Last())
          {
            case 'D':
              while (ka?.Length > i)
              {
                CI?.SetIsKeyPushed(ka[i], true);
                i++;
              }
              break;
            case 'U':
              while (ka?.Length > i)
              {
                CI?.SetIsKeyPushed(ka[i], false);
                i++;
              }
              break;
          }
          break;
        default:
          int Num = 0;
          if (!int.TryParse(GotStr.Substring(3), out Num)) break;
          switch (GotChars[2])
          {
            case 'B':
              BrakeNotchNum = Num;
              break;
            case 'P':
              PowerNotchNum = Num;
              break;
            case 'H':
              PowerNotchNum = -Num;
              break;
          }
          break;
      }

      return GotStr;
    }

    /// <summary>ヘッダがTRであるコマンドでの要求を処理する</summary>
    /// <param name="SVc">受信したインスタンス</param>
    /// <param name="GotString">接尾辞が取り除かれたコマンド文字列</param>
    /// <returns>返信すべき文字列(不要時はnull)</returns>
    static private string DataSelTR(IBIDSsv SVc, in string GotString)
    {
      if (SVc == null || string.IsNullOrWhiteSpace(GotString)) return Errors.GetCMD(Errors.ErrorNums.CMD_ERROR);
      string ReturnString = GotString + "X";//接尾辞は入っていないことを期待する.

      //0 1 2 3
      //T R X X
      switch (GotString.ToCharArray()[2])
      {
        case ConstVals.CMD_REVERSER://レバーサー
          int? revVal = GotString.Substring(3) switch
          {
            "R" => -1,
            "N" => 0,
            "F" => 1,
            "-1" => -1,
            "0" => 0,
            "1" => 1,
            _ => null
          };
          if (revVal == null) return Errors.GetCMD(Errors.ErrorNums.ERROR_in_DType_or_DNum);
          else
          {
            ReverserNum = revVal ?? 0;
            return ReturnString + revVal.ToString();
          }
        case ConstVals.CMD_SPoleMC://ワンハンドル
          int sers = 0;
          if (!int.TryParse(GotString.Substring(3), out sers)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);

          int pnn = 0;
          int bnn = 0;
          if (sers > 0) pnn = sers;
          if (sers < 0) bnn = -sers;
          PowerNotchNum = pnn;
          BrakeNotchNum = bnn;
          return ReturnString + "0";
        case ConstVals.CMD_POWER://Pノッチ操作
          int serp = 0;
          if (!int.TryParse(GotString.Substring(3), out serp)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);
          PowerNotchNum = serp;
          return ReturnString + "0";
        case ConstVals.CMD_BREAK://Bノッチ操作
          int serb = 0;
          if (!int.TryParse(GotString.Substring(3), out serb)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);
          BrakeNotchNum = serb;
          return ReturnString + "0";
        case ConstVals.CMD_KeyCtrl://キー操作
          int serk = 0;
          if (!int.TryParse(GotString.Substring(4), out serk)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);
          switch (GotString.Substring(3, 1))
          {
            //udpr
            case "U":
              //if (KyUp(serk)) return ReturnString + "0";
              //else return "TRE8";
              return "TRE3";
            case "D":
              //if (KyDown(serk)) return ReturnString + "0";
              //else return "TRE8";
              return "TRE3";
            case "P":
              if (0 <= serk && serk < 128) 
              {
                CI?.SetIsKeyPushed(serk, true);
                return ReturnString + "0";
              }
              else
              {
                return "TRE2";
              }
            case "R":
              if (0 <= serk && serk < 128) 
              {
                CI?.SetIsKeyPushed(serk, false);
                return ReturnString + "0";
              }
              else
              {
                return "TRE2";
              }
            default:
              return "TRE3";//記号部不正
          }
        case ConstVals.CMD_INFOREQ://情報取得
          char dtyp = '\0';
          int? dnum = null;

          //dtyp convert
          try
          {
            dtyp = GotString.ToCharArray()[ConstVals.DTYPE_CHAR_POS];
          }
          catch (Exception)
          {
            return Errors.GetCMD(Errors.ErrorNums.DTYPE_ERROR);
          }

          //dnum conv
          try
          {
            dnum = UFunc.String2INT(GotString.Substring(ConstVals.DNUM_POS));
          }
          catch (Exception)
          {
            return Errors.GetCMD(Errors.ErrorNums.DNUM_ERROR);
          }

          if (dnum == null) return Errors.GetCMD(Errors.ErrorNums.DNUM_ERROR);

          string s_ir = null;
          return Get_TRI_Data(out s_ir, dtyp, dnum ?? 0) ? ReturnString + s_ir : s_ir;
        case ConstVals.CMD_AUTOSEND_ADD://Auto Send Add
          int sera = 0;
          if (!int.TryParse(GotString.Substring(4), out sera)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);

          char dtypc_a = GotString.Substring(3, 1).ToCharArray()[0];
          ASList asl = null;
          switch (dtypc_a)
          {
            case ConstVals.DTYPE_PANEL:
            case ConstVals.DTYPE_PANEL_ARR:
              if (PDAutoList == null) PDAutoList = new ASList();
              asl = PDAutoList;
              break;
            case ConstVals.DTYPE_SOUND:
            case ConstVals.DTYPE_SOUND_ARR:
              if (SDAutoList == null) SDAutoList = new ASList();
              asl = SDAutoList;
              break;
            default:
              if (AutoNumL == null) AutoNumL = new ASList();
              asl = AutoNumL;
              break;
          }
          string asres = string.Empty;
          try
          {
            if (!Get_TRI_Data(out asres, dtypc_a, sera, true)) asres = "0";//BIDS SMem疎通状況によらず初期値を取得する.
          }
          catch (Exception)
          {
            return Errors.GetCMD(Errors.ErrorNums.UnknownError);
          }

          if (string.IsNullOrWhiteSpace(asres)) return Errors.GetCMD(Errors.ErrorNums.AS_AddErr);

          if (!asl.Contains(SVc, sera, dtypc_a)) asl.Add(SVc, sera, dtypc_a);

          return ReturnString + asres;
          
        case ConstVals.CMD_AUTOSEND_DEL://Auto Send Delete
          int serd;
          if (!int.TryParse(GotString.Substring(4), out serd)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);

          char dtypc_d = GotString.Substring(3, 1).ToCharArray()[0];
          ASList asld = null;
          ReturnString += "0";//値を入れて返すことはないから, 先に0を付けとく.
          switch (dtypc_d)//nullなら削除する要素は存在しない.
          {
            case ConstVals.DTYPE_PANEL:
            case ConstVals.DTYPE_PANEL_ARR:
              if (PDAutoList == null) return ReturnString;
              else asld = PDAutoList;
              break;
            case ConstVals.DTYPE_SOUND:
            case ConstVals.DTYPE_SOUND_ARR:
              if (SDAutoList == null) return ReturnString;
              else asld = SDAutoList;
              break;
            default:
              if (AutoNumL == null) return ReturnString;
              else asld = AutoNumL;
              break;
          }

          asld.Remove(SVc, serd, dtypc_d);
          return ReturnString;
          
        case ConstVals.CMD_ERROR:
          Console.WriteLine("BIDSsv.Common.DataSelTR : Error ({0}) got from {1}", GotString, SVc.Name);
          return null;

        case ConstVals.CMD_VERSION:
          int serv;
          if (!int.TryParse(GotString.Substring(3), out serv)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);
          if (serv < 0) Errors.GetCMD(Errors.ErrorNums.CMD_ERROR);
          SVc.Version = Math.Min(serv, Version);//デバイスのバージョンとBIDSsvのバージョンを比較し, より小さい値をその通信インスタンスでの採用バージョンとする.
          return ReturnString + SVc.Version.ToString();

        default:
          return "TRE4";//識別子不正
      }
    }

    /// <summary>TRIで始まるコマンドに対応するデータを返す</summary>
    /// <param name="ReturnStr">コマンドに対応するデータを格納する場所(先頭のセパレータは含まれません.)</param>
    /// <param name="DType">要求されたデータのタイプ</param>
    /// <param name="DNum">要求されたデータの番号</param>
    /// <param name="GetDataAnyway">BIDS SMemの疎通状況に依らずにデータを取得するか否か</param>
    /// <param name="separator">使用するセパレータ文字</param>
    /// <returns>エラー番号(なければnull)</returns>
    static public bool Get_TRI_Data(out string ReturnStr, in char DType, in int DNum, in bool GetDataAnyway = false, in char separator = ConstVals.CMD_SEPARATOR)
    {
			#region Local Function
      string ArrDGet(in int[] arr, in int dnum, in int DPerOneSending, in char separ)
      {
        if (dnum < 0) return DPerOneSending.ToString(ConstVals.ToStrFormatInt);//一度に送信する要素数
        string s = (((dnum * DPerOneSending) >= arr.Length) ? 0 : arr[dnum * DPerOneSending]).ToString(ConstVals.ToStrFormatInt);
        for (int i = (dnum * DPerOneSending) + 1; i < (dnum + 1) * DPerOneSending; i++)
          s += string.Format("{0}{1}", separ, (i >= arr.Length) ? 0 : arr[i]);
        return s;
      }
      #endregion

      //データ強制取得F && SMem未疎通 => NotConnected
      //データ強制取得F && SMem疎通済 => 値を返す
      //データ強制取得T && (疎通不依存) => 値を返す
      if (!GetDataAnyway && !BSMD.IsEnabled) {
        ReturnStr = Errors.GetCMD(Errors.ErrorNums.BIDS_Not_Connected);
        return false;
      }

      TimeSpan ts = TimeSpan.FromMilliseconds(BSMD.StateData.T);

			#region データ取得部
			ReturnStr = DType switch
      {
        ConstVals.DTYPE_CONSTD => (ConstVals.DNums.ConstD)DNum switch
        {
          ConstVals.DNums.ConstD.AllData => string.Format(StrFormat_BSMD_SpecAll, BSMD.SpecData.B, BSMD.SpecData.P, BSMD.SpecData.A, BSMD.SpecData.J, BSMD.SpecData.C),
          ConstVals.DNums.ConstD.Brake_Count => BSMD.SpecData.B.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.ConstD.Power_Count => BSMD.SpecData.P.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.ConstD.ATSCheckPos => BSMD.SpecData.A.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.ConstD.B67_Pos => BSMD.SpecData.J.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.ConstD.Car_Count => BSMD.SpecData.C.ToString(ConstVals.ToStrFormatInt),
          _ => Errors.GetCMD(Errors.ErrorNums.DNUM_ERROR)
        },
        ConstVals.DTYPE_ELAPD => (ConstVals.DNums.ElapD)DNum switch
        {
          ConstVals.DNums.ElapD.Time_HMSms => string.Format(StrFormat_BSMD_Time, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds),
          ConstVals.DNums.ElapD.Pressures => string.Format(StrFormat_BSMD_Pressures, BSMD.StateData.BC, BSMD.StateData.MR, BSMD.StateData.ER, BSMD.StateData.BP, BSMD.StateData.SAP),
          ConstVals.DNums.ElapD.AllData => string.Format(StrFormat_BSMD_StateAll, BSMD.StateData.Z, BSMD.StateData.V, BSMD.StateData.T, BSMD.StateData.BC, BSMD.StateData.MR, BSMD.StateData.ER, BSMD.StateData.BP, BSMD.StateData.SAP, BSMD.StateData.I, 0),
          ConstVals.DNums.ElapD.Distance => BSMD.StateData.Z.ToString(ConstVals.ToStrFormatFloat),
          ConstVals.DNums.ElapD.Speed => BSMD.StateData.V.ToString(ConstVals.ToStrFormatFloat),
          ConstVals.DNums.ElapD.Time => BSMD.StateData.T.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.ElapD.BC_Pres => BSMD.StateData.BC.ToString(ConstVals.ToStrFormatFloat),
          ConstVals.DNums.ElapD.MR_Pres => BSMD.StateData.MR.ToString(ConstVals.ToStrFormatFloat),
          ConstVals.DNums.ElapD.ER_Pres => BSMD.StateData.ER.ToString(ConstVals.ToStrFormatFloat),
          ConstVals.DNums.ElapD.BP_Pres => BSMD.StateData.BP.ToString(ConstVals.ToStrFormatFloat),
          ConstVals.DNums.ElapD.SAP_Pres => BSMD.StateData.SAP.ToString(ConstVals.ToStrFormatFloat),
          ConstVals.DNums.ElapD.Current => BSMD.StateData.I.ToString(ConstVals.ToStrFormatFloat),
          ConstVals.DNums.ElapD.Voltage => Errors.GetCMD(Errors.ErrorNums.DNUM_ERROR),
          ConstVals.DNums.ElapD.TIME_Hour => ts.Hours.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.ElapD.TIME_Min => ts.Minutes.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.ElapD.TIME_Sec => ts.Seconds.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.ElapD.TIME_MSec => ts.Milliseconds.ToString(ConstVals.ToStrFormatInt),
          _ => Errors.GetCMD(Errors.ErrorNums.DNUM_ERROR),
        },
        ConstVals.DTYPE_HANDPOS => (ConstVals.DNums.HandPos)DNum switch
        {
          ConstVals.DNums.HandPos.AllData => string.Format(StrFormat_BSMD_HandAll, BSMD.HandleData.B, BSMD.HandleData.P, BSMD.HandleData.R, BSMD.HandleData.C),
          ConstVals.DNums.HandPos.Brake => BSMD.HandleData.B.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.HandPos.Power => BSMD.HandleData.P.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.HandPos.Reverser => BSMD.HandleData.R.ToString(ConstVals.ToStrFormatInt),
          ConstVals.DNums.HandPos.ConstSpd => BSMD.HandleData.C.ToString(ConstVals.ToStrFormatInt),//定速状態は予約
          ConstVals.DNums.HandPos.SelfB => (GetDataAnyway || OD.IsEnabled) ? OD.SelfBPosition.ToString(ConstVals.ToStrFormatInt) : Errors.GetCMD(Errors.ErrorNums.BIDS_Not_Connected),
          _ => Errors.GetCMD(Errors.ErrorNums.DNUM_ERROR)
        },
        ConstVals.DTYPE_PANEL => DNum < 0 ? PD.Length.ToString(ConstVals.ToStrFormatInt) : (DNum < PD.Length ? PD.Panels[DNum] : 0).ToString(ConstVals.ToStrFormatInt),
        ConstVals.DTYPE_SOUND => DNum < 0 ? SD.Length.ToString(ConstVals.ToStrFormatInt) : (DNum < SD.Length ? SD.Sounds[DNum] : 0).ToString(ConstVals.ToStrFormatInt),
        ConstVals.DTYPE_DOOR => DNum switch
        {
          0 => BSMD.IsDoorClosed ? "1" : "0",
          1 => "0",
          2 => "0",
          _ => Errors.GetCMD(Errors.ErrorNums.DNUM_ERROR)
        },
        ConstVals.DTYPE_PANEL_ARR => ArrDGet(PD.Panels, DNum, ConstVals.PANEL_ARR_PRINT_COUNT, ConstVals.CMD_SEPARATOR),
        ConstVals.DTYPE_SOUND_ARR => ArrDGet(SD.Sounds, DNum, ConstVals.SOUND_ARR_PRINT_COUNT, ConstVals.CMD_SEPARATOR),
        _ => Errors.GetCMD(Errors.ErrorNums.DTYPE_ERROR)
      };
			#endregion

			return !ReturnStr.StartsWith(ConstVals.CMD_HEADER + ConstVals.CMD_ERROR);
    }

    static private byte[] DataSelBin(byte[] ba)
    {
      if (!(ba?.Length >= 6)) return null;//データ長6未満 or nullは対象外(念のためチェック)

      //Header Check
      if (ba[0] != ConstVals.BIN_CMD_HEADER_0 || ba[1] != ConstVals.BIN_CMD_HEADER_1) return null;//t:0x74, r:0x72

      switch (ba[2])
      {
        case ConstVals.BIN_CMD_INFO_DATA://Info Data
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
                _ = ba.GetFloat(i += 4);//WireVoltage
                s.T = ba.GetInt(i += 4);
                s.BC = ba.GetFloat(i += 4);
                s.MR = ba.GetFloat(i += 4);
                s.ER = ba.GetFloat(i += 4);
                s.BP = ba.GetFloat(i += 4);
                s.SAP = ba.GetFloat(i += 4);
                bsmd.IsDoorClosed = (ba[13 * 4] & 0b10000000) > 0;
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
        case ConstVals.BIN_CMD_PANEL_DATA://Panel Data
          if (ba.Length < AutoSendSetting.PanelSize) return null;
          int pai = 0;
          try
          {
            PanelD pd = PD;
            pai = Convert.ToInt32(ba[3]);
            if ((pai + 1) * 128 >= pd.Length)
            {
              int[] pa = new int[(pai + 1) * 128];
              Array.Copy(pd.Panels, pa, pd.Length);
              pd.Panels = pa;
            }
            Parallel.For(0, 128, (i) => pd.Panels[(pai * 128) + i] = ba.GetInt(4 + (4 * i)));
            PD = pd;
          }
          catch (ObjectDisposedException) { return null; }
          catch (Exception) { throw; }
          break;
        case ConstVals.BIN_CMD_SOUND_DATA://SoundData
          if (ba.Length < 129 * 4) return null;
          int sai = 0;
          try
          {
            SoundD sd = SD;
            sai = Convert.ToInt32(ba[3]);
            if ((sai + 1) * 128 >= sd.Length)
            {
              int[] sa = new int[(sai + 1) * 128];
              Array.Copy(sd.Sounds, sa, sd.Length);
              sd.Sounds = sa;
            }
            Parallel.For(0, 128, (i) => sd.Sounds[(sai * 128) + i] = ba.GetInt(4 + (4 * i)));
            SD = sd;
          }
          catch (ObjectDisposedException) { return null; }
          catch (Exception) { throw; }
          break;
        default:
          break;
      }

      return null;
    }

    /// <summary>Classify the data</summary>
    /// <param name="CName">Connection Instance</param>
    /// <param name="ba">Got Data</param>
    /// <param name="enc">Encording</param>
    /// <returns>byte array to return, or array that calling program is needed to do something</returns>
    static public byte[] DataSelect(IBIDSsv CName, in byte[] ba, in Encoding enc)
    {
      if (!(ba?.Length >= 4)) return null;//データ長4未満 or nullは対象外

      //StringデータかBinaryデータかを識別し, 適切なメソッドに処理を渡す

      if (ba[0] == (byte)'T') return enc.GetBytes(DataSelect(CName, enc.GetString(ba)) ?? string.Empty);
      else return DataSelBin(ba);
    }

    /// <summary>要求された情報を取得する</summary>
    /// <param name="sv">IBIDSsvインスタンス</param>
    /// <param name="str">入力されたコマンド文字列</param>
    /// <returns>返信すべき文字列</returns>
    static public string DataSelect(IBIDSsv sv, in string str)
    {
      if (sv == null) throw new ArgumentNullException();
      if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException();

      if (str.StartsWith(ConstVals.CMD_HEADER))
        if (str.Contains(ConstVals.CMD_SEPARATOR))//データ付き
        {
          DataGot(str);
          return null;
        }
        else return DataSelTR(sv, str);//データ要求
      
      else if (str.StartsWith(ConstVals.GIPI_HEADER)) return DataSelTO(str);

      return null;//対応しないコマンド
    }

    /// <summary>必要なデータを取得すると同時に, 送信まで行う.</summary>
    /// <param name="sv">通信インスタンス</param>
    /// <param name="data">入力データ</param>
    /// <param name="enc">使用するエンコーディング</param>
    static public void DataSelSend(IBIDSsv sv, in byte[] data, in Encoding enc) => sv?.Print(DataSelect(sv, data, enc));

    /// <summary>必要なデータを選択すると同時に, 送信まで行う.</summary>
    /// <param name="sv">IBIDSsvインスタンス</param>
    /// <param name="str">入力されたコマンド文字列</param>
    static public void DataSelSend(IBIDSsv sv, in string str) => sv?.Print(DataSelect(sv, str));
    


		#region Obsolete Method
		[Obsolete("Please use the \"DataSelect\" Method.")]
    static public string DataSelectTR(IBIDSsv CN, in string GotStr) => DataSelTR(CN, GotStr);

    [Obsolete("Please use the \"DataSelect\" Method.")]
    static public string DataSelectTO(in string GotString) => DataSelTO(GotString);
		#endregion
	}
}
