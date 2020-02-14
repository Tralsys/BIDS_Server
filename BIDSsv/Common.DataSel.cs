using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    static private string DataSelTR(in string CName, in string GotString)
    {
      if (IsDebug) Console.Write("{0} << {1}", CName, GotString);
      string ReturnString = GotString.Replace("\n", string.Empty) + "X";

      //0 1 2 3
      //T R X X
      switch (GotString.Substring(2, 1))
      {
        case "R"://レバーサー
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
        case "S"://ワンハンドル
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
        case "P"://Pノッチ操作
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
        case "B"://Bノッチ操作
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
        case "K"://キー操作
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
              if (serk < 128)
              {
                CI?.SetIsKeyPushed(serk, true);
                return ReturnString + "0";
              }
              else
              {
                return "TRE2";
              }
            case "R":
              if (serk < 128)
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
        case "I"://情報取得
          if (!BSMD.IsEnabled) return "TRE1";
          int seri = 0;
          try
          {
            seri = Convert.ToInt32(GotString.Substring(4));
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
            case "C":
              switch (seri)
              {
                case -1:
                  Spec spec = BSMD.SpecData;
                  return ReturnString + string.Format("{0}X{1}X{2}X{3}X{4}", spec.B, spec.P, spec.A, spec.J, spec.C);
                case 0:
                  return ReturnString + BSMD.SpecData.B.ToString();
                case 1:
                  return ReturnString + BSMD.SpecData.P.ToString();
                case 2:
                  return ReturnString + BSMD.SpecData.A.ToString();
                case 3:
                  return ReturnString + BSMD.SpecData.J.ToString();
                case 4:
                  return ReturnString + BSMD.SpecData.C.ToString();
                default: return "TRE2";
              }
            case "E":
              switch (seri)
              {
                case -3://Time
                  TimeSpan ts3 = TimeSpan.FromMilliseconds(BSMD.StateData.T);
                  return ReturnString + string.Format("{0}:{1}:{2}.{3}", ts3.Hours, ts3.Minutes, ts3.Seconds, ts3.Milliseconds);
                case -2://Pressure
                  State st2 = BSMD.StateData;
                  return ReturnString + string.Format("{0}X{1}X{2}X{3}X{4}", st2.BC, st2.MR, st2.ER, st2.BP, st2.SAP);
                case -1://All
                  State st1 = BSMD.StateData;
                  return ReturnString + string.Format(stateAllStr, st1.Z, st1.V, st1.T, st1.BC, st1.MR, st1.ER, st1.BP, st1.SAP, st1.I, 0);
                case 0: return ReturnString + BSMD.StateData.Z;
                case 1: return ReturnString + BSMD.StateData.V;
                case 2: return ReturnString + BSMD.StateData.T;
                case 3: return ReturnString + BSMD.StateData.BC;
                case 4: return ReturnString + BSMD.StateData.MR;
                case 5: return ReturnString + BSMD.StateData.ER;
                case 6: return ReturnString + BSMD.StateData.BP;
                case 7: return ReturnString + BSMD.StateData.SAP;
                case 8: return ReturnString + BSMD.StateData.I;
                //case 9: return ReturnString + BSMD.StateData.Volt;//予約 電圧
                case 10: return ReturnString + TimeSpan.FromMilliseconds(BSMD.StateData.T).Hours.ToString();
                case 11: return ReturnString + TimeSpan.FromMilliseconds(BSMD.StateData.T).Minutes.ToString();
                case 12: return ReturnString + TimeSpan.FromMilliseconds(BSMD.StateData.T).Seconds.ToString();
                case 13: return ReturnString + TimeSpan.FromMilliseconds(BSMD.StateData.T).Milliseconds.ToString();
                default: return "TRE2";
              }
            case "H":
              switch (seri)
              {
                case -1:
                  Hand hd1 = BSMD.HandleData;
                  return ReturnString + string.Format("{0}X{1}X{2}X{3}", hd1.B, hd1.P, hd1.R, hd1.C);
                case 0: return ReturnString + BSMD.HandleData.B;
                case 1: return ReturnString + BSMD.HandleData.P;
                case 2: return ReturnString + BSMD.HandleData.R;
                case 3: return ReturnString + BSMD.HandleData.C;//定速状態は予約
                case 4:
                  OpenD od = new OpenD();
                  SML?.Read(out od);
                  if (od.IsEnabled) return ReturnString + od.SelfBPosition.ToString();
                  else return "TRE1";//SMem is not connected.
                default: return "TRE2";
              }
            case "P":
              PanelD pd = new PanelD();
              SML?.Read(out pd);
              if (seri < 0) return ReturnString + pd.Length.ToString();
              else return ReturnString + (seri < pd.Length ? pd.Panels[seri] : 0).ToString();
            case "S":
              SoundD sd = new SoundD();
              SML?.Read(out sd);
              if (seri < 0) return ReturnString + sd.Length.ToString();
              else return ReturnString + (seri < sd.Length ? sd.Sounds[seri] : 0).ToString();
            case "D":
              switch (seri)
              {
                case 0: return ReturnString + (BSMD.IsDoorClosed ? "1" : "0");
                case 1: return ReturnString + "0";
                case 2: return ReturnString + "0";
                default: return "TRE2";
              }
            case "p":
              PanelD pda = new PanelD();
              SML?.Read(out pda);

              ReturnString += ((seri * 32) >= pda.Length) ? 0 : pda.Panels[seri * 32];
              for (int i = (seri * 32) + 1; i < (seri + 1) * 32; i++)
                ReturnString += "X" + ((i >= pda.Length) ? 0 : pda.Panels[i]);

              return ReturnString;
            case "s":
              SoundD sda = new SoundD();
              SML?.Read(out sda);
              ReturnString += ((seri * 32) >= sda.Length) ? 0 : sda.Sounds[seri * 32];
              for (int i = (seri * 32) + 1; i < (seri + 1) * 32; i++)
                ReturnString += "X" + ((i >= sda.Length) ? 0 : sda.Sounds[i]);

              return ReturnString;
            default: return "TRE3";//記号部不正
          }
        case "A"://Auto Send Add
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

          int Bias = -1;
          switch (GotString.Substring(3, 1))
          {
            case "C":
              Bias = 0;
              break;
            case "H":
              Bias = HandDBias;
              break;
            case "D":
              Bias = DoorDBias;
              break;
            case "E":
              Bias = ElapDBias;
              break;
            case "P":
              if (PDAutoList?.Contains(new KeyValuePair<string, int>(CName, sera)) != true) PDAutoList.Add(CName, sera);
              return ReturnString + (SML.Panels.Length > sera ? SML.Panels.Panels[sera] : 0).ToString();
            case "S":
              if (SDAutoList?.Contains(new KeyValuePair<string, int>(CName, sera)) != true) SDAutoList.Add(CName, sera);
              return ReturnString + (SML.Sounds.Length > sera ? SML.Sounds.Sounds[sera] : 0).ToString();
          }


          if (Bias >= 0)
          {
            if (AutoNumL?.Contains(new KeyValuePair<string, int>(CName, Bias + sera)) != true) AutoNumL.Add(CName, Bias + sera);
            return ReturnString + "0";
          }
          else return "TRE3";
        case "D"://Auto Send Delete
          int Biasd = -1;
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

          switch (GotString.Substring(3, 1))
          {
            case "C":
              Biasd = 0;
              break;
            case "H":
              Biasd = HandDBias;
              break;
            case "D":
              Biasd = DoorDBias;
              break;
            case "E":
              Biasd = ElapDBias;
              break;
            case "P":
              if (PDAutoList.Contains(new KeyValuePair<string, int>(CName, serd))) PDAutoList.Remove(new KeyValuePair<string, int>(CName, serd));
              return ReturnString + "0";
            case "S":
              if (!SDAutoList.Contains(new KeyValuePair<string, int>(CName, serd))) SDAutoList.Remove(new KeyValuePair<string, int>(CName, serd));
              return ReturnString + "0";
          }

          if (Biasd > 0)
          {
            if (AutoNumL.Contains(new KeyValuePair<string, int>(CName, Biasd + serd))) AutoNumL.Remove(new KeyValuePair<string, int>(CName, Biasd + serd));

            return ReturnString + "0";
          }
          else return "TRE3";
        case "E":
        //throw new Exception(GotString);
        default:
          return "TRE4";//識別子不正
      }
    }

    /// <summary>Classify the data</summary>
    /// <param name="CName">Connection Name</param>
    /// <param name="data">Got Data</param>
    /// <param name="enc">Encording</param>
    /// <returns>byte array to return, or array that calling program is needed to do something</returns>
    static public byte[] DataSelect(in string CName, in byte[] data, in Encoding enc)
    {
      if (data == null || data.Length < 4) return null;
      byte[] ba = BIDSBAtoBA(data);
      if (ba[0] == (byte)'T')
      {
        switch ((char)ba[1])
        {
          case 'R':
            string gs = enc.GetString(ba);
            if (gs != null && gs != string.Empty)
            {
              if (gs.Contains("X")) DataGot(gs);
              else
              {
                string sr = DataSelTR(CName, gs);
                if (sr != null && sr != string.Empty) return enc.GetBytes(sr);
              }
            }
            break;
          case 'O':
            string so = DataSelTO(enc.GetString(ba));
            if (so != null && so != string.Empty) return enc.GetBytes(so);
            break;
        }
      }
      else if (ba[0] == 't' && ba[1] == 'r')//t:0x74, r:0x72
      {
        if (ba.Length < 6) return null;
        switch (ba[2])
        {
          case 0x62://Info Data
            switch (ba[3])
            {
              case 0x01://Spec
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
                else return ba;
                break;
              case 0x02://State
                if (ba.Length >= AutoSendSetting.BasicCommonSize)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  State s = bsmd.StateData;
                  int i = 0;
                  s.Z = ba.GetDouble(i += 4);
                  s.V = ba.GetFloat(i += 8);
                  s.I = ba.GetFloat(i += 4);
                  //s. = ba.GetFloat(i += 4).Take(4).ToArray());WireVoltage
                  s.BC = ba.GetFloat(i += 8);
                  s.MR = ba.GetFloat(i += 4);
                  s.ER = ba.GetFloat(i += 4);
                  s.BP = ba.GetFloat(i += 4);
                  s.SAP = ba.GetFloat(i += 4);
                  bsmd.IsDoorClosed = (ba[13 * 4] & 0b10000000) > 0;
                  bsmd.StateData = s;
                  BSMD = bsmd;
                }
                else return ba;
                break;
              case 0x03://BVE5D
                break;
              case 0x04://OpenD
                break;
              case 0x05://Handle
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
          case 0x70://Panel Data
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
          case 0x73://SoundData
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
            return data;
        }
      }
      return null;
    }


    [Obsolete("Please use the \"DataSelect\" Method.")]
    static public string DataSelectTR(in string CN, in string GotStr) => DataSelTR(CN, GotStr);

    [Obsolete("Please use the \"DataSelect\" Method.")]
    static public string DataSelectTO(in string GotString) => DataSelTO(GotString);


  }
}
