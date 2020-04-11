using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
  static public partial class Common
  {
    static public readonly int Version = 202;
    static public readonly int DefPNum = 14147;


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

    static public Hands Ctrl_Hand
    {
      get => CI?.GetHandD() ?? new Hands();
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

    static private List<IBIDSsv> svlist = new List<IBIDSsv>();
    static private SMemLib SML = null;
    static private CtrlInput CI = null;

    static private bool IsStarted = false;
    static private bool IsDebug = false;

    static string stateAllStr = "{0}";

    static public void Start(int Interval = 10, bool NO_SMEM_MODE = false)
    {
      if (!IsStarted)
      {
        for (int i = 1; i <= 9; i++) stateAllStr += ("X{" + i.ToString() + "}");
        SML = new SMemLib(true, 0, NO_SMEM_MODE);
        CI = new CtrlInput();
        SML?.ReadStart(0, Interval);

        BSMDChanged += Common_BSMDChanged;
        OpenDChanged += Common_OpenDChanged;
        PanelDChanged += Common_PanelDChanged;
        SoundDChanged += Common_SoundDChanged;
        IsStarted = true;
      }
    }
    
    static public void Add<T>(ref T container) where T : IBIDSsv => svlist.Add(container);
    static public void Remove() => Remove(null);
    static public void Remove(IBIDSsv Name)
    {
      if (!(svlist?.Count > 0)) return;
      ASRemove(Name);
      for(int i = svlist.Count - 1; i >= 0; i--)
      {
        if (Name == null || svlist[i] == Name)
        {
          try
          {
            svlist[i]?.Dispose();
            svlist.RemoveAt(i);
          }catch(Exception e)
          {
            Console.WriteLine(e);
          }
        }
      }
    }
    static public void Remove(in IBIDSsv sv)
    {
      if (sv == null || !(svlist?.Count > 0)) return;

      ASRemove(sv);

      Console.WriteLine("BIDSsv.Common : {0} Remove {1}", sv.Name, svlist.Remove(sv) ? "done." : "failed (not found?)");
    }

    static private void ASRemove(IBIDSsv Name)
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

    static public bool Print(string Name, string Command)
    {
      if (Name != null && Name != string.Empty)
      {
        for (int i = svlist.Count - 1; i >= 0; i--)
          if (Name == svlist[i].Name)
          {
            try
            {
              svlist[i].Print(Command);
            }
            catch (Exception e)
            {
              Console.WriteLine(e);
              return false;
            }
          }
      }
      else return false;

      return true;
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

    static private void DataGot(in string GotStr)
    {
      if (GotStr == null || GotStr == string.Empty) return;
      if (!GotStr.StartsWith("TR")) return;
      string[] GSA = GotStr.Replace("\n", string.Empty).Split('X');
      if (GSA[0].StartsWith("TRI"))
      {
        int seri = 0;
        try
        {
          seri = Convert.ToInt32(GotStr.Substring(4));
        }
        catch (FormatException)
        {
          throw new Exception("TRE6");//要求情報コード 文字混入
        }
        catch (OverflowException)
        {
          throw new Exception("TRE5");//要求情報コード 変換オーバーフロー
        }
        switch (GotStr.Substring(3, 1))
        {
          case "C":
            switch (seri)
            {
              case -1:
                if (GSA.Length > 5)
                {
                  Spec spec;
                  try
                  {
                    spec.B = int.Parse(GSA[1]);
                    spec.P = int.Parse(GSA[2]);
                    spec.A = int.Parse(GSA[3]);
                    spec.J = int.Parse(GSA[4]);
                    spec.C = int.Parse(GSA[5]);
                    BIDSSharedMemoryData bsmd = BSMD;
                    bsmd.SpecData = spec;
                    BSMD = bsmd;
                  }
                  catch (Exception) { throw; }
                }
                break;
              case 0:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.B = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              case 1:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.P = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              case 2:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.A = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              case 3:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.J = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              case 4:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.C = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              default: throw new Exception("TRE2");
            }
            break;
          case "E":
            switch (seri)
            {
              case -3://Time
                if (GSA.Length > 4)
                {
                  TimeSpan ts3 = new TimeSpan(0, int.Parse(GSA[1]), int.Parse(GSA[2]), int.Parse(GSA[3]), int.Parse(GSA[4]));
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.T = (int)ts3.TotalMilliseconds;
                  BSMD = bsmd;
                }
                break;
              case -2://Pressure
                if (GSA.Length > 5)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  int i = 1;
                  bsmd.StateData.BC = float.Parse(GSA[i++]);
                  bsmd.StateData.MR = float.Parse(GSA[i++]);
                  bsmd.StateData.ER = float.Parse(GSA[i++]);
                  bsmd.StateData.BP = float.Parse(GSA[i++]);
                  bsmd.StateData.SAP = float.Parse(GSA[i++]);
                  BSMD = bsmd;
                }
                break;
              case -1://All
                if (GSA.Length > 10)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  State st = bsmd.StateData;
                  int i = 1;
                  st.Z = double.Parse(GSA[i++]);
                  st.V = float.Parse(GSA[i++]);
                  st.T = int.Parse(GSA[i++]);
                  st.BC = float.Parse(GSA[i++]);
                  st.MR = float.Parse(GSA[i++]);
                  st.ER = float.Parse(GSA[i++]);
                  st.BP = float.Parse(GSA[i++]);
                  st.SAP = float.Parse(GSA[i++]);
                  st.I = float.Parse(GSA[i++]);
                }
                break;
              case 0:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.Z = double.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 1:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.V = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 2:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.T = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 3:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.BC = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 4:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.MR = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 5:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.ER = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 6:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.BP = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 7:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.SAP = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 8:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.I = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              //case 9: return ReturnString + BSMD.StateData.Volt;//予約 電圧
              case 10://Hour
                break;
              case 11:
                break;
              case 12:
                break;
              case 13:
                break;
              default: throw new Exception("TRE2");
            }
            break;
          case "H":
            switch (seri)
            {
              case -1:
                if (GSA.Length > 4)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.B = int.Parse(GSA[1]);
                  bsmd.HandleData.P = int.Parse(GSA[2]);
                  bsmd.HandleData.R = int.Parse(GSA[3]);
                  bsmd.HandleData.C = int.Parse(GSA[4]);
                  BSMD = bsmd;
                }
                break;
              case 0:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.B = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 1:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.P = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 2:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.R = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 3:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.C = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 4:
                if (GSA.Length > 1)
                {
                  OpenD opd = OD;
                  opd.SelfBPosition = int.Parse(GSA[1]);
                  OD = opd;
                }
                break;
              default: break;
            }
            break;
          case "P":
            if (GSA.Length > 1 && seri >= 0 && seri < PD.Length)
              PD.Panels[seri] = int.Parse(GSA[1]);
            break;
          case "S":
            if (GSA.Length > 1 && seri >= 0 && seri < SD.Length)
              SD.Sounds[seri] = int.Parse(GSA[1]);
            break;
          case "D":
            switch (seri)
            {
              case 0:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.IsDoorClosed = GSA[1] == "1";
                  BSMD = bsmd;
                }
                break;
              default: break;
            }
            break;
          case "p":
            if (GSA.Length > 32 && seri >= 0)
            {
              int mx = (seri + 1) * 32;
              int[] pda;
              if (PD.Length >= mx)
              {
                pda = new int[mx];
                Array.Copy(PD.Panels, pda, PD.Length);
              }
              else pda = PD.Panels;

              for (int i = seri * 32; i < mx; i++)
                if (i < PD.Length) pda[i] = int.Parse(GSA[(i % 32) + 1]);
              PanelD pd = new PanelD() { Panels = pda };
              PD = pd;
            }
            break;
          case "s":
            if (GSA.Length > 32 && seri >= 0)
            {
              int mx = (seri + 1) * 32;
              int[] sda;
              if (SD.Length >= mx)
              {
                sda = new int[mx];
                Array.Copy(SD.Sounds, sda, SD.Length);
              }
              else sda = SD.Sounds;

              for (int i = seri * 32; i < mx; i++)
                if (i < SD.Length) sda[i] = int.Parse(GSA[(i % 32) + 1]);
              SoundD sd = new SoundD() { Sounds = sda };
              SD = sd;
            }
            break;
          default: throw new Exception("TRE3");//記号部不正
        }
      }
    }

    /// <summary>(disabled)Convert from Native Byte Array to BIDS Communication Byte Array Format</summary>
    /// <param name="ba">Array to Converted</param>
    /// <returns>Converted Array</returns>
    static public byte[] BAtoBIDSBA(in byte[] ba)
    {
      return ba;
      /*List<byte> dbl = ba.ToList();
      for(int i = 0; i < ba.Count(); i++)
      {
        switch (dbl[i])
        {
          case (byte)'\n':
            dbl.RemoveAt(i);
            byte[] r1 = new byte[2] { (byte)'\r', 0x01 };
            dbl.InsertRange(i, r1);
            i++;
            break;
          case (byte)'\r':
            i++;
            dbl.Insert(i, 0x02);
            break;
        }
      }
      dbl.Add((byte)'\n');
      return dbl.ToArray();*/
    }
    /// <summary>(disabled)Convert from BIDS Communication Byte Array Format to Native Byte Array</summary>
    /// <param name="ba">Array to Converted</param>
    /// <returns>Converted Array</returns>
    static public byte[] BIDSBAtoBA(in byte[] ba)
    {
      return ba;
      /*if (ba.Length > 1)
      {
        List<byte> dbl = ba.ToList();
        //dbl.RemoveAt(dbl.Count - 1);
        for (int i = 0; i < dbl.Count - 1; i++)
        {
          if (dbl[i] == '\r')
          {
            switch (dbl[i + 1])
            {
              case 0x01:
                dbl[i] = (byte)'\n';
                dbl.RemoveAt(i + 1);
                break;
              case 0x02:
                dbl.RemoveAt(i + 1);
                break;
            }
          }
        }
        return dbl.ToArray();
      }
      else return ba;*/
    }
  }
}
