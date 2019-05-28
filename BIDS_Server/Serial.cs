using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
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
    public void OnBSMDChanged(in BIDSSharedMemoryData data)
    {
      //throw new NotImplementedException();
    }

    public void OnOpenDChanged(in OpenD data)
    {
      //throw new NotImplementedException();
    }

    public void OnPanelDChanged(in int[] data)
    {
      //throw new NotImplementedException();
    }

    public void OnSoundDChanged(in int[] data)
    {
      //throw new NotImplementedException();
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
                //定速状態は予約
                default: return "TRE2";
              }
            case "P":
              if (seri < 0) return "TRE2";
              PanelD pd;
              SML?.Read(out pd);
              return ReturnString + (seri < pd.Length ? pd.Panels[seri] : 0).ToString();
            case "S":
              if (seri > 255 || seri < 0) return "TRE2";
              SoundD sd;
              SML?.Read(out sd);
              return ReturnString + (seri < sd.Length ? sd.Sounds[seri] : 0).ToString();
            case "D":
              if (BSMD.IsDoorClosed) return ReturnString + "0";
              else return ReturnString + "1";
            default: return "TRE3";//記号部不正
          }
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
