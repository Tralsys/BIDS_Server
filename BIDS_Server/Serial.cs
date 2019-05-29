using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TR.BIDSSMemLib;
using TR.BIDSsv;

namespace BIDS_Server
{
  public class Serial : IBIDSsv
  {
    public Serial()
    {

    }

    public int Version => 202;
    public bool IsDebug { get; set; } = false;
    public string Name { get; set; } = "serial";

    public event EventHandler<HandleCtrlEvArgs> HandleCtrl;
    public event EventHandler<KeyCtrlEvArgs> KeyCtrl;
    public event EventHandler BSMDChanged;
    public event EventHandler OpenDChanged;
    public event EventHandler PanelDChanged;
    public event EventHandler SoundDChanged;
    SerialPort SP = null;
    bool IsLooping = true;
    SMemLib SML = null;
    CtrlInput CI = null;
    int ReverserNum
    {
      get => CI?.GetHandD().R ?? 0;
      set => CI?.SetHandD(CtrlInput.HandType.Reverser, value);
    }
    int BrakeNotchNum
    {
      get => CI?.GetHandD().B ?? 0;
      set => CI?.SetHandD(CtrlInput.HandType.Brake, value);
    }
    int PowerNotchNum
    {
      get => CI?.GetHandD().P ?? 0;
      set => CI?.SetHandD(CtrlInput.HandType.Power, value);
    }
    Thread ReadThread = null;

    const int OpenDBias = 1000000;
    const int ElapDBias = 100000;
    const int DoorDBias = 10000;
    const int HandDBias = 1000;

    public bool Connect(in string args)
    {
      SP = new SerialPort();
      SP.BaudRate = 19200;
      SP.RtsEnable = true;
      SP.DtrEnable = true;
      SP.ReadTimeout = 100;
      SP.WriteTimeout = 1000;
      SP.Encoding = Encoding.Default;
      string[] sa = args.Replace(" ", string.Empty).Split(new string[2] { "-", "/" }, StringSplitOptions.RemoveEmptyEntries);
      for(int i = 0; i < sa.Length; i++)
      {
        string[] saa = sa[i].Split(':');
        if (saa.Length > 0)
        {
          switch (saa[0])
          {
            case "B":
              SP.BaudRate = int.Parse(saa[1]);
              break;
            case "BaudRate":
              SP.BaudRate = int.Parse(saa[1]);
              break;
            case "DataBits":
              SP.DataBits = int.Parse(saa[1]);
              break;
            case "DTR":
              SP.DtrEnable = saa[1] == "1";
              break;
            case "E":
              switch (int.Parse(saa[1]))
              {
                case 0:
                  SP.Encoding = Encoding.Default;
                  break;
                case 1:
                  SP.Encoding = Encoding.ASCII;
                  break;
                case 2:
                  SP.Encoding = Encoding.Unicode;
                  break;
                case 3:
                  SP.Encoding = Encoding.UTF8;
                  break;
                case 4:
                  SP.Encoding = Encoding.UTF32;
                  break;
                default:
                  SP.Encoding = Encoding.Default;
                  break;
              }
              break;
            case "Encoding":
              switch (int.Parse(saa[1]))
              {
                case 0:
                  SP.Encoding = Encoding.Default;
                  break;
                case 1:
                  SP.Encoding = Encoding.ASCII;
                  break;
                case 2:
                  SP.Encoding = Encoding.Unicode;
                  break;
                case 3:
                  SP.Encoding = Encoding.UTF8;
                  break;
                case 4:
                  SP.Encoding = Encoding.UTF32;
                  break;
                default:
                  SP.Encoding = Encoding.Default;
                  break;
              }
              break;
            case "HandShake":
              SP.Handshake = (Handshake)int.Parse(saa[1]);
              break;
            case "N":
              Name = saa[1];
              break;
            case "Name":
              Name = saa[1];
              break;
            case "P":
              SP.PortName = "COM" + int.Parse(saa[1]);
              break;
            case "Port":
              SP.PortName = "COM" + int.Parse(saa[1]);
              break;
            case "Parity":
              SP.Parity = (Parity)int.Parse(saa[1]);
              break;
            case "RTO":
              SP.ReadTimeout = int.Parse(saa[1]);
              break;
            case "ReadTimeout":
              SP.ReadTimeout = int.Parse(saa[1]);
              break;
            case "RTS":
              SP.RtsEnable = saa[1] == "1";
              break;
            case "StopBits":
              SP.StopBits = (StopBits)int.Parse(saa[1]);
              break;
            case "WTO":
              SP.WriteTimeout = int.Parse(saa[1]);
              break;
            case "WriteTimeout":
              SP.WriteTimeout = int.Parse(saa[1]);
              break;
          }
        }
      }
      SP?.Open();
      if (SP?.IsOpen == true)
      {
        ReadThread = new Thread(ReadDoing);
        ReadThread.Start();
        CI = new CtrlInput();
        SML = new SMemLib();
      }
      Console.WriteLine(Name + " : " + SP?.IsOpen.ToString());
      return SP?.IsOpen == true;
    }

    public void Dispose()
    {
      IsLooping = false;
      SML?.Dispose();
      if (ReadThread?.Join(5000) == false) Console.WriteLine(Name + " : ReadThread Closing Failed.");
      CI?.Dispose();
      SP?.Dispose();
      Console.WriteLine(Name + " : " + SP?.IsOpen.ToString());
    }
    private void ReadDoing()
    {
      while (SP?.IsOpen == true && IsLooping)
      {
        string inputStr = null;
        try
        {
          inputStr = SP?.ReadLine();
        }
        catch (TimeoutException) { continue; }
        catch(Exception e)
        {
          Console.WriteLine("Error has occured at ReadDoing on" + Name);
          Console.WriteLine(e);
          continue;
        }
        if (inputStr != null && inputStr.Length > 4)
        {
          if (IsDebug) Console.WriteLine(Name + " : Get > " + inputStr);
          if (inputStr.StartsWith("TO")) SPWriteLine(DataSelectTO(in inputStr));
          if (inputStr.StartsWith("TR"))
          {
            if (inputStr.StartsWith("TRV"))
            {
              if (int.Parse(inputStr.Substring(3, 3)) >= 202)
              {
                if (inputStr.Contains("BR"))
                {
                  int BaR = int.Parse(inputStr.Split(new string[] { "BR" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                  SPWriteLine(inputStr + "X202" + BaR.ToString());
                  SP.BaudRate = BaR;
                }
                else SPWriteLine(inputStr + "X202");
              }
              else SPWriteLine(inputStr + "X" + inputStr.Substring(3));
            }
            else SPWriteLine(DataSelectTR(in inputStr));
          }
        }
      }
    }
    void SPWriteLine(string s)
    {
      if (SP != null)
      {
        if (IsDebug) Console.WriteLine(Name + " : Set > " + s);
        try
        {
          SP.WriteLine(s);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      }
    }


    BIDSSharedMemoryData oldBSMD = new BIDSSharedMemoryData();
    List<int> AutoNumL = new List<int>();
    OpenD oldOD = new OpenD();
    PanelD oldPD = new PanelD();
    List<int> PDAutoList = new List<int>();
    SoundD oldSD = new SoundD();
    List<int> SDAutoList = new List<int>();
    public void OnBSMDChanged(in BIDSSharedMemoryData data)
    {
      bool IsDClsd = data.IsDoorClosed;
      Spec osp = oldBSMD.SpecData;
      Spec nsp = data.SpecData;
      State ost = oldBSMD.StateData;
      State nst = data.StateData;
      Hand oh = oldBSMD.HandleData;
      Hand nh = data.HandleData;
      TimeSpan ots = TimeSpan.FromMilliseconds(oldBSMD.StateData.T);
      TimeSpan nts = TimeSpan.FromMilliseconds(data.StateData.T);
      Parallel.ForEach(AutoNumL, (i) =>
      {
        string WriteStr = string.Empty;

        
        string chr = string.Empty;
        int num = 0;
        if (OpenDBias > i && i >= ElapDBias)
        {
          switch (i - ElapDBias)
          {
            case 0: WriteStr = Conp(ost.Z, nst.Z); break;
            case 1: WriteStr = Conp(ost.V, nst.V); break;
            case 2: WriteStr = Conp(ost.T, nst.T); break;
            case 3: WriteStr = Conp(ost.BC, nst.BC); break;
            case 4: WriteStr = Conp(ost.MR, nst.MR); break;
            case 5: WriteStr = Conp(ost.ER, nst.ER); break;
            case 6: WriteStr = Conp(ost.BP, nst.BP); break;
            case 7: WriteStr = Conp(ost.SAP, nst.SAP); break;
            case 8: WriteStr = Conp(ost.I, nst.I); break;
            //case 9: WriteStr = Conp(ost.Z, nst.Z); break;
            case 10: WriteStr = Conp(ots.Hours, nts.Hours); break;
            case 11: WriteStr = Conp(ots.Minutes, nts.Minutes); break;
            case 12: WriteStr = Conp(ots.Seconds, nts.Seconds); break;
            case 13: WriteStr = Conp(ots.Milliseconds, nts.Milliseconds); break;
          }
          (chr, num) = ("E", i - ElapDBias);
        }
        else if (i >= DoorDBias)
        {
          switch (i - DoorDBias)
          {
            case 0: WriteStr = Conp(oldBSMD.IsDoorClosed ? 1 : 0, IsDClsd ? 1 : 0); break;
          }
          (chr, num) = ("D", i - DoorDBias);
        }
        else if (i >= HandDBias)
        {
          switch (i - HandDBias)
          {
            case 0: WriteStr = Conp(oh.B, nh.B); break;
            case 1: WriteStr = Conp(oh.P, nh.P); break;
            case 2: WriteStr = Conp(oh.R, nh.R); break;
            case 3: WriteStr = Conp(oh.C, nh.C); break;
          }
          (chr, num) = ("H", i - HandDBias);
        }
        else if (OpenDBias > i)
        {
          switch (i)
          {
            case 0: WriteStr = Conp(osp.B, nsp.B); break;
            case 1: WriteStr = Conp(osp.P, nsp.P); break;
            case 2: WriteStr = Conp(osp.A, nsp.A); break;
            case 3: WriteStr = Conp(osp.J, nsp.J); break;
            case 4: WriteStr = Conp(osp.C, nsp.C); break;
          }
          (chr, num) = ("C", i % HandDBias);
        }
        if (WriteStr != string.Empty) SP?.WriteLine("TRI" + chr + num.ToString() + "X" + WriteStr);
      });
      oldBSMD = data;
    }

    private static string Conp(object oldobj, object newobj) => Equals(oldobj, newobj) ? newobj.ToString() : string.Empty;

    public void OnOpenDChanged(in OpenD data)
    {
      oldOD = data;
    }

    public void OnPanelDChanged(in int[] data)
    {
      if (PDAutoList.Count > 0)
      {
        for (int i = 0; i < PDAutoList.Count; i++)
        {
          int ind = PDAutoList[i];
          bool OldIndIs = ind < (oldPD.Panels != null ? oldPD.Length : 0);
          bool NewIndIs = ind < data.Length;
          int RetVal = -1;
          if (OldIndIs && NewIndIs) { if (oldPD.Panels[ind] != data[ind]) RetVal = data[ind]; }//Old:True, New:True
          else if (!OldIndIs) RetVal = NewIndIs ? data[ind] : -1;//Old:False / New:True=>data, New:False=>-1
          else if (!NewIndIs) RetVal = 0;//Old:True, New:False => 0

          if (RetVal >= 0) SP?.WriteLine("TRIP" + ind.ToString() + "X" + RetVal.ToString());
        }
      }
      oldPD.Panels = data;
    }

    public void OnSoundDChanged(in int[] data)
    {
      if (oldSD.Sounds != null)
      {
        if (SDAutoList.Count > 0)
        {
          for (int i = 0; i < SDAutoList.Count; i++)
          {
            int ind = SDAutoList[i];
            bool OldIndIs = ind < (oldSD.Sounds != null ? oldSD.Length : 0);
            bool NewIndIs = ind < data.Length;
            int RetVal = -1;
            if (OldIndIs && NewIndIs) { if (oldSD.Sounds[ind] != data[ind]) RetVal = data[ind]; }//Old:True, New:True
            else if (!OldIndIs) RetVal = NewIndIs ? data[ind] : -1;//Old:False / New:True=>data, New:False=>-1
            else if (!NewIndIs) RetVal = 0;//Old:True, New:False => 0

            if (RetVal >= 0) SP?.WriteLine("TRIP" + ind.ToString() + "X" + RetVal.ToString());
          }
        }
      }
      oldSD.Sounds = data;
    }


    readonly string[] ArgInfo = new string[]
    {
      "Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
      "  -B or -BaudRate : Set BaudRate.  If you don\'t set this command, the BaudRate is set to 19200.",
      "  -DataBits : Set the DataBit Count Option.  Default:8  If you want More info about this argument, please read the source code.",
      "  -DTR : Set the RTS (Data Terminal Ready) Signal Option.  Default:1  0:Disabled, 1:Enabled",
      "  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -HandShake : Set the HandShake Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -N or -Name : Set the Instance Name.  Default:\"serial\"  If you don't set this option, this program maybe cause some bugs.",
      "  -P or -PortName : Set PortName.  You must set this option.  If you forgot setting this option, you can not open the port.  \"COM\" is unneeded in the Setting.  Only Number is allowed in the setting.",
      "  -Parity : Set the Parity Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -RTO or ReadTimeout : Set the ReadTimeout time option.  Default:100",
      "  -RTS : Set the RTS (Request to Send) Signal Option.  Default:1  0:Disabled, 1:Enabled",
      "  -StopBits : Set the StopBits Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -WTO or WriteTimeOut : Set the WriteTimeout time option  Default:1000"
    };
    public void WriteHelp(in string args)
    {
      Console.WriteLine("BIDS Server Program Serial Interface");
      Console.WriteLine("Version : " + Version.ToString());
      Console.WriteLine("Copyright © Tetsu Otter 2019");
      foreach (string s in ArgInfo) Console.WriteLine(s);

    }


    private string DataSelectTR(in string GetString)
    {
      string ReturnString = string.Empty;
      ReturnString = GetString + "X";
      //0 1 2 3
      //T R X X
      switch (GetString.Substring(2, 1))
      {
        case "R"://レバーサー
          switch (GetString.Substring(3))
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
            sers = Convert.ToInt32(GetString.Substring(3));
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
            serp = Convert.ToInt32(GetString.Substring(3));
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
            serb = Convert.ToInt32(GetString.Substring(3));
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
            serk = Convert.ToInt32(GetString.Substring(4));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          switch (GetString.Substring(3, 1))
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
          BIDSSharedMemoryData BSMD = (BIDSSharedMemoryData?)SML?.Read<BIDSSharedMemoryData>() ?? new BIDSSharedMemoryData();
          if (!BSMD.IsEnabled) return "TRE1";
          int seri = 0;
          try
          {
            seri = Convert.ToInt32(GetString.Substring(4));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          switch (GetString.Substring(3, 1))
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
            case "P":;
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
            sera = Convert.ToInt32(GetString.Substring(4));
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
          switch (GetString.Substring(3, 1))
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
              if (!PDAutoList.Contains(sera)) PDAutoList.Add(sera);
              return ReturnString + "0";
            case "S":
              if (!SDAutoList.Contains(sera)) SDAutoList.Add(sera);
              return ReturnString + "0";
          }


          if (Bias >= 0)
          {
            if (!AutoNumL.Contains(Bias + sera)) AutoNumL.Add(Bias + sera);
            return ReturnString + "0";
          }
          else return "TRE3";
        case "D"://Auto Send Delete
          int Biasd = -1;
          int serd = 0;

          try
          {
            serd = Convert.ToInt32(GetString.Substring(4));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }

          switch (GetString.Substring(3, 1))
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
              if (!PDAutoList.Contains(serd)) PDAutoList.Remove(serd);
              return ReturnString + "0";
            case "S":
              if (!SDAutoList.Contains(serd)) SDAutoList.Remove(serd);
              return ReturnString + "0";
          }

          if (Biasd > 0)
          {
            if (AutoNumL.Contains(Biasd + serd)) AutoNumL.Remove(Biasd + serd);
            return ReturnString + "0";
          }
          else return "TRE3";
        default:
          return "TRE4";//識別子不正
      }
    }
    private string DataSelectTO(in string GetString)
    {
      string ThirdStr = GetString.Substring(2, 1);
      if (ThirdStr == "R")
      {
        switch (GetString.Substring(3, 1))
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
          KNum = Convert.ToInt32(GetString.Substring(3).Replace("D", string.Empty).Replace("U", string.Empty));
        }
        catch (FormatException)
        {
          return "TRE6";//要求情報コード 文字混入
        }
        catch (OverflowException)
        {
          return "TRE5";//要求情報コード 変換オーバーフロー
        }
        if (GetString.EndsWith("D"))
        {
          CI?.SetIsKeyPushed(KNum, true);
        }
        if (GetString.EndsWith("U"))
        {
          CI?.SetIsKeyPushed(KNum, false);
        }
      }
      else
      {
        int Num = 0;
        try
        {
          Num = Convert.ToInt32(GetString.Substring(3));
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
      return GetString;
    }
  }
}
