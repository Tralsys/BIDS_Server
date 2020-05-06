using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
  static public partial class Common
  {
    static readonly byte[] ZeroX4 = new byte[]{ 0, 0, 0, 0 };

    static private string DataSelTO(in string GotStr)
    {
      string GotString = GotStr.Replace("\n", string.Empty);
      string ThirdStr = GotString.Substring(2, 1);
      if (ThirdStr == "R")
      {
        switch (GotString.Substring(3, 1))
        {
          case "F":
            ReverserNum = 1;
            break;
          case "N":
            ReverserNum = 0;
            break;
          case "R":
            ReverserNum = -1;
            break;
          case "B":
            ReverserNum = -1;
            break;
        }
      }
      else if (ThirdStr == "K")
      {
        int KNum = 0;
        int[] ka = null;
        try
        {
          KNum = Convert.ToInt32(GotString.Substring(3).Replace("D", string.Empty).Replace("U", string.Empty));
          ka = GIPI.GetBtJobNum(KNum);
          if (!(ka?.Length > 0)) return GotString;
          KNum = 0;
        }
        catch (FormatException)
        {
          return "TRE6";//要求情報コード 文字混入
        }
        catch (OverflowException)
        {
          return "TRE5";//要求情報コード 変換オーバーフロー
        }
        if (GotString.EndsWith("D"))
        {
          while (ka?.Length > KNum)
          {
            CI?.SetIsKeyPushed(ka[KNum], true);
            KNum++;
          }
        }
        if (GotString.EndsWith("U"))
        {
          while (ka?.Length > KNum)
          {
            CI?.SetIsKeyPushed(ka[KNum], false);
            KNum++;
          }
        }
      }
      else
      {
        int Num = 0;
        try
        {
          Num = Convert.ToInt32(GotString.Substring(3));
        }
        catch (FormatException)
        {
          return "TRE6";//要求情報コード 文字混入
        }
        catch (OverflowException)
        {
          return "TRE5";//要求情報コード 変換オーバーフロー
        }
        switch (ThirdStr)
        {
          case "B":
            BrakeNotchNum = Num;
            break;
          case "P":
            PowerNotchNum = Num;
            break;
          case "H":
            PowerNotchNum = -Num;
            break;
        }
      }
      return GotString;
    }

    static private string DataSelTR(IBIDSsv SVc, in string GotString)
    {
      string ReturnString = GotString.Replace("\n", string.Empty) + "X";

      //0 1 2 3
      //T R X X
      switch (GotString.ToCharArray()[2])
      {
        case ConstVals.CMD_REVERSER://レバーサー
          switch (GotString.Substring(3))
          {
            case "R":
              ReverserNum = -1;
              break;
            case "N":
              ReverserNum = 0;
              break;
            case "F":
              ReverserNum = 1;
              break;
            case "-1":
              ReverserNum = -1;
              break;
            case "0":
              ReverserNum = 0;
              break;
            case "1":
              ReverserNum = 1;
              break;
            default:
              return "TRE7";//要求情報コードが不正
          }
          return ReturnString + "0";
        case ConstVals.CMD_SPoleMC://ワンハンドル
          int sers = 0;
          try
          {
            sers = Convert.ToInt32(GotString.Substring(3));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          int pnn = 0;
          int bnn = 0;
          if (sers > 0) pnn = sers;
          if (sers < 0) bnn = -sers;
          PowerNotchNum = pnn;
          BrakeNotchNum = bnn;
          return ReturnString + "0";
        case ConstVals.CMD_POWER://Pノッチ操作
          int serp = 0;
          try
          {
            serp = Convert.ToInt32(GotString.Substring(3));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          PowerNotchNum = serp;
          return ReturnString + "0";
        case ConstVals.CMD_BREAK://Bノッチ操作
          int serb = 0;
          try
          {
            serb = Convert.ToInt32(GotString.Substring(3));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          BrakeNotchNum = serb;
          return ReturnString + "0";
        case ConstVals.CMD_KeyCtrl://キー操作
          int serk = 0;
          try
          {
            serk = Convert.ToInt32(GotString.Substring(4));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
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

          try
          {
            return ReturnString + Get_TRI_Data(dtyp, dnum ?? 0);
          }
          catch(BIDSErrException bee)
          {
            return bee.ErrCMD;
          }
          catch (Exception) { throw; }
        case ConstVals.CMD_AUTOSEND_ADD://Auto Send Add
          int sera = 0;
          try
          {
            sera = Convert.ToInt32(GotString.Substring(4));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }

          char dtypc_a = GotString.Substring(3, 1).ToCharArray()[0];
          ASList asl = null;
          switch (dtypc_a)
          {
            case ConstVals.DTYPE_PANEL:
              if (PDAutoList == null) PDAutoList = new ASList(false);
              asl = PDAutoList;
              break;
            case ConstVals.DTYPE_SOUND:
              if (SDAutoList == null) SDAutoList = new ASList(false);
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
            asres = Get_TRI_Data(dtypc_a, sera, true);//BIDS SMem疎通状況によらず初期値を取得する.
          }
          catch (BIDSErrException)
          {
            asres = "0";
          }//無視(使用できないデータでも一応受け付けておく.)
          catch (Exception)
          {
            return Errors.GetCMD(Errors.ErrorNums.UnknownError);
          }

          if (string.IsNullOrWhiteSpace(asres)) return Errors.GetCMD(Errors.ErrorNums.AS_AddErr);

          if (!asl.Contains(SVc, sera, dtypc_a)) asl.Add(SVc, sera, dtypc_a);

          return ReturnString + asres;
          
        case ConstVals.CMD_AUTOSEND_DEL://Auto Send Delete
          int serd;
          try
          {
            serd = Convert.ToInt32(GotString.Substring(4));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }

          char dtypc_d = GotString.Substring(3, 1).ToCharArray()[0];
          ASList asld = null;
          ReturnString += "0";//値を入れて返すことはないから, 先に0を付けとく.
          switch (dtypc_d)//nullなら削除する要素は存在しない.
          {
            case ConstVals.DTYPE_PANEL:
              if (PDAutoList == null) return ReturnString;
              else asld = PDAutoList;
              break;
            case ConstVals.DTYPE_SOUND:
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
          //throw new Exception(GotString);
          return null;

        case ConstVals.CMD_VERSION:
          int serv = UFunc.String2INT(GotString.Substring(3)) ?? -1;
          if (serv < 0) Errors.GetCMD(Errors.ErrorNums.CMD_ERROR);
          SVc.Version = Math.Min(serv, Version);//デバイスのバージョンとBIDSsvのバージョンを比較し, より小さい値をその通信インスタンスでの採用バージョンとする.
          return ReturnString + SVc.Version.ToString();

        default:
          return "TRE4";//識別子不正
      }
    }

    /// <summary>TRIで始まるコマンドに対応するデータを返す</summary>
    /// <param name="DType">要求されたデータのタイプ</param>
    /// <param name="DNum">要求されたデータの番号</param>
    /// <param name="GetDataAnyway">BIDS SMemの疎通状況に依らずにデータを取得するか否か</param>
    /// <param name="separator">使用するセパレータ文字</param>
    /// <returns>要求されたデータ文字列(先頭のセパレータは含まれません.)</returns>
    static public string Get_TRI_Data(char DType, int DNum, bool GetDataAnyway = false, char separator = ConstVals.CMD_SEPARATOR)
    {
      //データ強制取得F && SMem未疎通 => NotConnected
      //データ強制取得F && SMem疎通済 => 値を返す
      //データ強制取得T && (疎通不依存) => 値を返す
      if (!GetDataAnyway && !BSMD.IsEnabled) throw new BIDSErrException(Errors.ErrorNums.BIDS_Not_Connected);

      string s = "{0}";//連続出力用フォーマット指定文字列
      switch (DType)
      {
        case ConstVals.DTYPE_CONSTD:
          switch ((ConstVals.DNums.ConstD)DNum)
          {
            case ConstVals.DNums.ConstD.AllData:
              Spec spec = BSMD.SpecData;
              for (int i = 1; i < 5; i++)
                s += string.Format("{0}{{1}}", separator, i);
              
              return string.Format(s, spec.B, spec.P, spec.A, spec.J, spec.C);
            case ConstVals.DNums.ConstD.Brake_Count:
              return BSMD.SpecData.B.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.ConstD.Power_Count:
              return BSMD.SpecData.P.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.ConstD.ATSCheckPos:
              return BSMD.SpecData.A.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.ConstD.B67_Pos:
              return BSMD.SpecData.J.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.ConstD.Car_Count:
              return BSMD.SpecData.C.ToString(ConstVals.ToStrFormatInt);
            default: throw new BIDSErrException(Errors.ErrorNums.DNUM_ERROR);
          }
        case ConstVals.DTYPE_ELAPD:
          switch ((ConstVals.DNums.ElapD)DNum)
          {
            case ConstVals.DNums.ElapD.Time_HMSms://Time
              TimeSpan ts3 = TimeSpan.FromMilliseconds(BSMD.StateData.T);
              return string.Format("{0}:{1}:{2}.{3}", ts3.Hours, ts3.Minutes, ts3.Seconds, ts3.Milliseconds);
            case ConstVals.DNums.ElapD.Pressures://Pressure
              State st2 = BSMD.StateData;
              for (int i = 1; i < 5; i++)
                s += string.Format("{0}{{1}}", separator, i);
              return string.Format(s, st2.BC, st2.MR, st2.ER, st2.BP, st2.SAP);
            case ConstVals.DNums.ElapD.AllData://All
              State st1 = BSMD.StateData;
              for (int i = 1; i < 10; i++)
                s += string.Format("{0}{{1}}", separator, i);
              return string.Format(s, st1.Z, st1.V, st1.T, st1.BC, st1.MR, st1.ER, st1.BP, st1.SAP, st1.I, 0);
            case ConstVals.DNums.ElapD.Distance: return BSMD.StateData.Z.ToString(ConstVals.ToStrFormatFloat);
            case ConstVals.DNums.ElapD.Speed: return BSMD.StateData.V.ToString(ConstVals.ToStrFormatFloat);
            case ConstVals.DNums.ElapD.Time: return BSMD.StateData.T.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.ElapD.BC_Pres: return BSMD.StateData.BC.ToString(ConstVals.ToStrFormatFloat);
            case ConstVals.DNums.ElapD.MR_Pres: return BSMD.StateData.MR.ToString(ConstVals.ToStrFormatFloat);
            case ConstVals.DNums.ElapD.ER_Pres: return BSMD.StateData.ER.ToString(ConstVals.ToStrFormatFloat);
            case ConstVals.DNums.ElapD.BP_Pres: return BSMD.StateData.BP.ToString(ConstVals.ToStrFormatFloat);
            case ConstVals.DNums.ElapD.SAP_Pres: return BSMD.StateData.SAP.ToString(ConstVals.ToStrFormatFloat);
            case ConstVals.DNums.ElapD.Current: return BSMD.StateData.I.ToString(ConstVals.ToStrFormatFloat);
            //case 9: return BSMD.StateData.Volt;//予約 電圧
            case ConstVals.DNums.ElapD.TIME_Hour: return TimeSpan.FromMilliseconds(BSMD.StateData.T).Hours.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.ElapD.TIME_Min: return TimeSpan.FromMilliseconds(BSMD.StateData.T).Minutes.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.ElapD.TIME_Sec: return TimeSpan.FromMilliseconds(BSMD.StateData.T).Seconds.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.ElapD.TIME_MSec: return TimeSpan.FromMilliseconds(BSMD.StateData.T).Milliseconds.ToString(ConstVals.ToStrFormatInt);
            default: throw new BIDSErrException(Errors.ErrorNums.DNUM_ERROR);
          }
        case ConstVals.DTYPE_HANDPOS:
          switch ((ConstVals.DNums.HandPos)DNum)
          {
            case ConstVals.DNums.HandPos.AllData:
              Hand hd1 = BSMD.HandleData;
              for (int i = 1; i < 4; i++)
                s += string.Format("{0}{{1}}", separator, i);
              return string.Format(s, hd1.B, hd1.P, hd1.R, hd1.C);
            case ConstVals.DNums.HandPos.Brake: return BSMD.HandleData.B.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.HandPos.Power: return BSMD.HandleData.P.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.HandPos.Reverser: return BSMD.HandleData.R.ToString(ConstVals.ToStrFormatInt);
            case ConstVals.DNums.HandPos.ConstSpd: return BSMD.HandleData.C.ToString(ConstVals.ToStrFormatInt);//定速状態は予約
            case ConstVals.DNums.HandPos.SelfB:
              OpenD od = new OpenD();
              SML?.Read(out od);
              if (GetDataAnyway || od.IsEnabled) return od.SelfBPosition.ToString(ConstVals.ToStrFormatInt);
              else throw new BIDSErrException(Errors.ErrorNums.BIDS_Not_Connected);//SMem is not connected.
            default: throw new BIDSErrException(Errors.ErrorNums.DNUM_ERROR);
          }
        case ConstVals.DTYPE_PANEL:
          PanelD pd = new PanelD();
          SML?.Read(out pd);
          if (DNum < 0) return pd.Length.ToString(ConstVals.ToStrFormatInt);
          else return (DNum < pd.Length ? pd.Panels[DNum] : 0).ToString(ConstVals.ToStrFormatInt);
        case ConstVals.DTYPE_SOUND:
          SoundD sd = new SoundD();
          SML?.Read(out sd);
          if (DNum < 0) return sd.Length.ToString(ConstVals.ToStrFormatInt);
          else return (DNum < sd.Length ? sd.Sounds[DNum] : 0).ToString(ConstVals.ToStrFormatInt);
        case ConstVals.DTYPE_DOOR:
          switch (DNum)
          {
            case 0: return BSMD.IsDoorClosed ? "1" : "0";
            case 1: return "0";
            case 2: return "0";
            default: throw new BIDSErrException(Errors.ErrorNums.DNUM_ERROR);
          }
        case ConstVals.DTYPE_PANEL_ARR:
          if (DNum < 0) return ConstVals.PANEL_ARR_PRINT_COUNT.ToString(ConstVals.ToStrFormatInt);//負の数は一度に出力する数の指定
          PanelD pda = new PanelD();
          SML?.Read(out pda);
          string pReturnStr = string.Empty;
          pReturnStr += ((DNum * ConstVals.PANEL_ARR_PRINT_COUNT) >= pda.Length) ? 0 : pda.Panels[DNum * ConstVals.PANEL_ARR_PRINT_COUNT];
          for (int i = (DNum * ConstVals.PANEL_ARR_PRINT_COUNT) + 1;
            i < (DNum + 1) * ConstVals.PANEL_ARR_PRINT_COUNT; i++)
            pReturnStr += string.Format("{0}{1}",separator, (i >= pda.Length) ? 0 : pda.Panels[i]);

          return pReturnStr;
        case ConstVals.DTYPE_SOUND_ARR:
          if (DNum < 0) return ConstVals.SOUND_ARR_PRINT_COUNT.ToString(ConstVals.ToStrFormatInt);//負の数は一度に出力する数の指定
          SoundD sda = new SoundD();
          SML?.Read(out sda);
          string sReturnStr = string.Empty;
          sReturnStr += ((DNum * ConstVals.SOUND_ARR_PRINT_COUNT) >= sda.Length) ? 0 : sda.Sounds[DNum * ConstVals.SOUND_ARR_PRINT_COUNT];
          for (int i = (DNum * ConstVals.SOUND_ARR_PRINT_COUNT) + 1;
            i < (DNum + 1) * ConstVals.SOUND_ARR_PRINT_COUNT; i++)
            sReturnStr += string.Format("{0}{1}", separator, (i >= sda.Length) ? 0 : sda.Sounds[i]);

          return sReturnStr;
        default: throw new BIDSErrException(Errors.ErrorNums.DTYPE_ERROR);//記号部不正
      }

      throw new BIDSErrException(Errors.ErrorNums.ERROR_in_DType_or_DNum);
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
