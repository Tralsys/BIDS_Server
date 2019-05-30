using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.BIDSSMemLib;
using TR.BIDSsv;

namespace BIDS_Server
{
  static public class UFunc
  {
    public static string Comp(object oldobj, object newobj) => Equals(oldobj, newobj) ? string.Empty : newobj.ToString();
  }
  static public class Common
  {
    static public BIDSSharedMemoryData BSMD
    {
      get => (BIDSSharedMemoryData)SML?.Read<BIDSSharedMemoryData>();
      set => SML?.Write(in value);
    }
    static public OpenD OD
    {
      get => (OpenD)SML?.Read<OpenD>();
      set => SML?.Write(in value);
    }
    static public PanelD PD
    {
      get => (PanelD)SML?.Read<PanelD>();
      set => SML?.Write(in value);
    }
    static public SoundD SD
    {
      get => (SoundD)SML?.Read<SoundD>();
      set => SML?.Write(in value);
    }

    static public Hand Ctrl_Hand
    {
      get => CI?.GetHandD() ?? new Hand();
      set => CI?.SetHandD(ref value);
    }
    static public bool[] Ctrl_Key
    {
      get => CI?.GetIsKeyPushed() ?? new bool[128];
      set => CI?.SetIsKeyPushed(in value);
    }

    static public int PowerNotchNum
    {
      get => Ctrl_Hand.P;
      set => CI?.SetHandD(CtrlInput.HandType.Power, value);
    }
    static public int BrakeNotchNum
    {
      get => Ctrl_Hand.B;
      set => CI?.SetHandD(CtrlInput.HandType.Brake, value);
    }
    static public int ReverserNum
    {
      get => Ctrl_Hand.R;
      set => CI?.SetHandD(CtrlInput.HandType.Reverser, value);
    }

    static public event EventHandler<SMemLib.BSMDChangedEArgs> BSMDChanged
    {
      add => SMemLib.BIDSSMemChanged += value;
      remove => SMemLib.BIDSSMemChanged -= value;
    }
    static public event EventHandler<SMemLib.OpenDChangedEArgs> OpenDChanged
    {
      add => SMemLib.OpenDChanged += value;
      remove => SMemLib.OpenDChanged -= value;
    }
    static public event EventHandler<SMemLib.ArrayDChangedEArgs> PanelDChanged
    {
      add => SMemLib.PanelDChanged += value;
      remove => SMemLib.PanelDChanged -= value;
    }
    static public event EventHandler<SMemLib.ArrayDChangedEArgs> SoundDChanged
    {
      add => SMemLib.SoundDChanged += value;
      remove => SMemLib.SoundDChanged -= value;
    }


    const int OpenDBias = 1000000;
    const int ElapDBias = 100000;
    const int DoorDBias = 10000;
    const int HandDBias = 1000;

    static private List<IBIDSsv> svlist = new List<IBIDSsv>();
    static private IDictionary<string, int> PDAutoList = default;
    static private IDictionary<string, int> SDAutoList = default;
    static private IDictionary<string, int> AutoNumL = default;
    static private SMemLib SML = null;
    static private CtrlInput CI = null;

    static private bool IsStarted = false;
    static private bool IsDebug = false;

    static public void Start(int Interval = 10)
    {
      if (!IsStarted)
      {
        SML = new SMemLib(true, 0);
        CI = new CtrlInput();
        SML?.ReadStart(5, Interval);

        BSMDChanged += Common_BSMDChanged;
        OpenDChanged += Common_OpenDChanged;
        PanelDChanged += Common_PanelDChanged;
        SoundDChanged += Common_SoundDChanged;
        IsStarted = true;
      }
    }

    private static void Common_SoundDChanged(object sender, SMemLib.ArrayDChangedEArgs e)
    {
      if (!IsStarted) return;
      if (svlist?.Count > 0) Parallel.For(0, svlist.Count, (i) => svlist[i].OnSoundDChanged(in e.NewArray));

      if (SDAutoList.Count > 0 && svlist.Count > 0)
      {
        Parallel.For(0, Math.Max(e.OldArray.Length, e.NewArray.Length), (i) =>
        {
          if (SDAutoList.Values.Contains(i))
          {
            int? Num = null;
            if (e.OldArray.Length <= i) Num = e.NewArray[i];
            else if (e.NewArray.Length > i && e.OldArray[i] != e.NewArray[i]) Num = e.NewArray[i];

            if (Num != null)
            {
              Parallel.For(0, svlist.Count, (s) =>
              {
                if (SDAutoList.Contains(new KeyValuePair<string, int>(svlist[s].Name, i)))
                  svlist[s].Print("TRIS" + i.ToString() + "X" + Num.ToString());
              });
            }
          }
        });
      }
    }
    private static void Common_PanelDChanged(object sender, SMemLib.ArrayDChangedEArgs e)
    {
      if (!IsStarted) return;
      if (svlist?.Count > 0) Parallel.For(0, svlist.Count, (i) => svlist[i].OnPanelDChanged(in e.NewArray));

      if (PDAutoList.Count > 0 && svlist.Count > 0)
      {
        Parallel.For(0, Math.Max(e.OldArray.Length, e.NewArray.Length), (i) =>
        {
          if (PDAutoList.Values.Contains(i))
          {
            int? Num = null;
            if (e.OldArray.Length <= i) Num = e.NewArray[i];
            else if (e.NewArray.Length > i && e.OldArray[i] != e.NewArray[i]) Num = e.NewArray[i];

            if (Num != null)
            {
              Parallel.For(0, svlist.Count, (s) =>
              {
                if (PDAutoList.Contains(new KeyValuePair<string, int>(svlist[s].Name, i)))
                  svlist[s].Print("TRIP" + i.ToString() + "X" + Num.ToString());
              });
            }
          }
        });
      }
    }
    private static void Common_OpenDChanged(object sender, SMemLib.OpenDChangedEArgs e) { if (!IsStarted) return; if (svlist?.Count > 0) Parallel.For(0, svlist.Count, (i) => svlist[i].OnOpenDChanged(in e.NewData)); }
    private static void Common_BSMDChanged(object sender, SMemLib.BSMDChangedEArgs e)
    {
      if (!IsStarted) return;
      if (svlist?.Count > 0) Parallel.For(0, svlist.Count, (i) => svlist[i].OnBSMDChanged(in e.NewData));

      if (AutoNumL?.Count > 0 && svlist?.Count > 0)
      {
        bool IsDClsdo = e.OldData.IsDoorClosed;
        bool IsDClsd = e.NewData.IsDoorClosed;
        Spec osp = e.OldData.SpecData;
        Spec nsp = e.NewData.SpecData;
        State ost = e.OldData.StateData;
        State nst = e.NewData.StateData;
        Hand oh = e.OldData.HandleData;
        Hand nh = e.NewData.HandleData;
        TimeSpan ots = TimeSpan.FromMilliseconds(e.OldData.StateData.T);
        TimeSpan nts = TimeSpan.FromMilliseconds(e.NewData.StateData.T);
        ICollection<int> IC = AutoNumL.Values;
        ICollection<int> ICR = default;
        Parallel.For(0, IC.Count, (ind) =>
        {
          int i = IC.ElementAt(ind);
          if (!ICR.Contains(i))
          {
            ICR.Add(i);

            string WriteStr = string.Empty;
            string chr = string.Empty;
            int num = 0;

            if (OpenDBias > i && i >= ElapDBias)
            {
              switch (i - ElapDBias)
              {
                case 0: WriteStr = UFunc.Comp(ost.Z, nst.Z); break;
                case 1: WriteStr = UFunc.Comp(ost.V, nst.V); break;
                case 2: WriteStr = UFunc.Comp(ost.T, nst.T); break;
                case 3: WriteStr = UFunc.Comp(ost.BC, nst.BC); break;
                case 4: WriteStr = UFunc.Comp(ost.MR, nst.MR); break;
                case 5: WriteStr = UFunc.Comp(ost.ER, nst.ER); break;
                case 6: WriteStr = UFunc.Comp(ost.BP, nst.BP); break;
                case 7: WriteStr = UFunc.Comp(ost.SAP, nst.SAP); break;
                case 8: WriteStr = UFunc.Comp(ost.I, nst.I); break;
                //case 9: WriteStr = UFunc.Comp(ost.Z, nst.Z); break;
                case 10: WriteStr = UFunc.Comp(ots.Hours, nts.Hours); break;
                case 11: WriteStr = UFunc.Comp(ots.Minutes, nts.Minutes); break;
                case 12: WriteStr = UFunc.Comp(ots.Seconds, nts.Seconds); break;
                case 13: WriteStr = UFunc.Comp(ots.Milliseconds, nts.Milliseconds); break;
              }
              (chr, num) = ("E", i - ElapDBias);
            }
            else if (i >= DoorDBias)
            {
              switch (i - DoorDBias)
              {
                case 0: WriteStr = UFunc.Comp(IsDClsdo ? 1 : 0, IsDClsd ? 1 : 0); break;
              }
              (chr, num) = ("D", i - DoorDBias);
            }
            else if (i >= HandDBias)
            {
              switch (i - HandDBias)
              {
                case 0: WriteStr = UFunc.Comp(oh.B, nh.B); break;
                case 1: WriteStr = UFunc.Comp(oh.P, nh.P); break;
                case 2: WriteStr = UFunc.Comp(oh.R, nh.R); break;
                case 3: WriteStr = UFunc.Comp(oh.C, nh.C); break;
              }
              (chr, num) = ("H", i - HandDBias);
            }
            else if (OpenDBias > i)
            {
              switch (i)
              {
                case 0: WriteStr = UFunc.Comp(osp.B, nsp.B); break;
                case 1: WriteStr = UFunc.Comp(osp.P, nsp.P); break;
                case 2: WriteStr = UFunc.Comp(osp.A, nsp.A); break;
                case 3: WriteStr = UFunc.Comp(osp.J, nsp.J); break;
                case 4: WriteStr = UFunc.Comp(osp.C, nsp.C); break;
              }
              (chr, num) = ("C", i % HandDBias);
            }


            if (WriteStr != string.Empty) 
            {
              Parallel.For(0, svlist.Count, (s) =>
              {
                if (SDAutoList.Contains(new KeyValuePair<string, int>(svlist[s].Name, i)))
                  svlist[s].Print("TRIS" + num.ToString() + "X" + WriteStr);
              });
            }
          }
        });
      }
    }

    static internal void Add<T>(ref T container) where T : IBIDSsv => svlist.Add(container);
    static internal void Remove() => Remove(string.Empty);
    static internal void Remove(in string Name)
    {
      if (Name != string.Empty)
      {
        try
        {
          if (PDAutoList?.Count > 0) PDAutoList.Remove(Name);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
        try
        {
          if (SDAutoList?.Count > 0) SDAutoList.Remove(Name);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
        try
        {
          if (AutoNumL?.Count > 0) AutoNumL.Remove(Name);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
        for (int i = 0; i < svlist.Count; i++)
          if (Name == svlist[i].Name)
          {
            try
            {
              svlist[i].Dispose();
              svlist.RemoveAt(i);
            }
            catch (Exception e)
            {
              Console.WriteLine(e);
            }
            return;
          }
      }
      else
      {
        if (svlist.Count > 0) for (int i = 0; i < svlist.Count; i++) svlist[i]?.Dispose();
      }
    }
    static public void DebugDo() => DebugDo(string.Empty);
    static public void DebugDo(in string Name)
    {
      if (svlist.Count > 0)
      {
        Console.WriteLine("Debug Start");
        for (int i = 0; i < svlist.Count; i++)
          if (Name == string.Empty || Name == svlist[i].Name) svlist[i].IsDebug = true;
        IsDebug = Name == string.Empty;
        Console.ReadKey();
        IsDebug = false;
        for (int i = 0; i < svlist.Count; i++) svlist[i].IsDebug = false;
        Console.WriteLine("Debug End");
      }
      else Console.WriteLine("DebugDo : There are no connection.");
    }

    static public string PrintList()
    {
      string s = string.Empty;
      if (svlist?.Count > 0) for (int i = 0; i < svlist.Count; i++) s += i.ToString() + " : " + svlist[i].Name + "\n";
      else s = "There are no connection.\n";
      return s;
    }

    static public void Dispose()
    {
      if (svlist.Count > 0) Parallel.For(0, svlist.Count, (i) => svlist[i].Dispose());
      SML?.ReadStop();
      SML?.Dispose();
    }



    static public string DataSelectTR(in string CName, in string GotString)
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
              ;
              PanelD pd;
              SML?.Read(out pd);
              if (seri < 0) return ReturnString + pd.Length.ToString();
              else return ReturnString + (seri < pd.Length ? pd.Panels[seri] : 0).ToString();
            case "S":
              SoundD sd;
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
              if (!PDAutoList.Contains(new KeyValuePair<string, int>(CName, sera))) PDAutoList.Add(CName, sera);
              return ReturnString + "0";
            case "S":
              if (!SDAutoList.Contains(new KeyValuePair<string, int>(CName, sera))) SDAutoList.Add(CName, sera);
              return ReturnString + "0";
          }


          if (Bias >= 0)
          {
            if (!AutoNumL.Contains(new KeyValuePair<string, int>(CName, Bias + sera))) AutoNumL.Add(CName, Bias + sera);
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
    static public string DataSelectTO(in string GotStr)
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
        try
        {
          KNum = Convert.ToInt32(GotString.Substring(3).Replace("D", string.Empty).Replace("U", string.Empty));
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
          CI?.SetIsKeyPushed(KNum, true);
        }
        if (GotString.EndsWith("U"))
        {
          CI?.SetIsKeyPushed(KNum, false);
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
    static public void DataGot(in string GotStr)
    {
      if (GotStr == null || GotStr == string.Empty) return;
      if (!GotStr.StartsWith("TR") && !GotStr.StartsWith("TO")) return;
      string[] GSA = GotStr.Replace("\n", string.Empty).Split('X');
      switch (GSA[0].Substring(2, 1))
      {
        case "I":
          if (GSA[0].Substring(3, 1) == "D") if (IsDebug) Console.WriteLine(">>>{0}", GotStr);
          break;
      }
    }
  }
}
