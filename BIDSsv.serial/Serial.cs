using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TR.BIDSSMemLib;
using TR.BIDSsv;

namespace TR.BIDSsv
{
  public class Serial : IBIDSsv
  {
    public int Version => 202;
    public bool IsDebug { get; set; } = false;
    public string Name { get; private set; } = "serial";

    SerialPort SP = null;
    bool IsLooping = true;

    Thread ReadThread = null;

    public bool Connect(in string args)
    {
      SP = new SerialPort();

      SP.ReadBufferSize = 64;
      SP.WriteBufferSize = 64;

      SP.BaudRate = 19200;
      SP.RtsEnable = true;
      SP.DtrEnable = true;
      SP.ReadTimeout = 20;
      SP.WriteTimeout = 500;
      SP.Encoding = Encoding.Default;
      SP.NewLine = "\n";
      string[] sa = args.Replace(" ", string.Empty).Split(new string[2] { "-", "/" }, StringSplitOptions.RemoveEmptyEntries);
      for (int i = 0; i < sa.Length; i++)
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
      }
      Console.WriteLine(Name + " : " + SP?.IsOpen.ToString());
      return SP?.IsOpen == true;
    }

    public void Dispose()
    {
      IsLooping = false;
      if (ReadThread?.Join(5000) == false) Console.WriteLine(Name + " : ReadThread Closing Failed.");
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
        catch (TimeoutException)
        {
          try
          {
            inputStr = SP?.ReadTo("\r");
          }
          catch (TimeoutException)
          {
            continue;
          }catch(Exception e)
          {
            Console.WriteLine("Error has occured at ReadDoing(\\r) on" + Name);
            Console.WriteLine(e);
          }

        }
        catch (Exception e)
        {
          Console.WriteLine("Error has occured at ReadDoing(\\n) on" + Name);
          Console.WriteLine(e);
          continue;
        }
        if (inputStr != null && inputStr.Length > 3)
        {
          if (IsDebug) Console.WriteLine(Name + " : Get > " + inputStr + "\n");
          //byte[] debugBA = SP.Encoding.GetBytes(inputStr);
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
          Thread.Sleep(10);
          SP.WriteLine(s);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      }
    }

    public void OnBSMDChanged(in BIDSSharedMemoryData data) { }
    public void OnOpenDChanged(in OpenD data) { }
    public void OnPanelDChanged(in int[] data) { }
    public void OnSoundDChanged(in int[] data) { }


    readonly string[] ArgInfo = new string[]
    {
      "Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
      "  -B or -BaudRate : Set BaudRate.  If you don\'t set this command, the BaudRate is set to 19200.",
      "  -DataBits : Set the DataBit Count Option.  Default:8  If you want More info about this argument, please read the source code.",
      "  -DTR : Set the RTS (Data Terminal Ready) Signal Option.  Default:1  0:Disabled, 1:Enabled",
      "  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -HandShake : Set the HandShake Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -N or -Name : Set the Instance Name.  Default:\"serial\"  If you don't set this option, this program maybe cause some bugs.",
      "  -P or -PortName : Set PortName.  You must set this option.  \"COM\" is unneeded in the Setting.  Only Number is allowed in the setting.",
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
      Console.WriteLine("Copyright (C) Tetsu Otter 2019");
      foreach (string s in ArgInfo) Console.WriteLine(s);

    }


    private string DataSelectTR(in string GetString) => Common.DataSelectTR(Name, in GetString);
    private string DataSelectTO(in string GetString) => Common.DataSelectTO(in GetString);

    public void Print(in string data) => SPWriteLine(data);
    public void Print(in byte[] data)
    {
      /*byte[] wa = Common.BAtoBIDSBA(data);
      if (data[data.Length - 1] != (byte)'\n')
      {
        wa = new byte[data.Length + 1];
        Array.Copy(data, wa, data.Length);
        wa[data.Length] = (byte)'\n';
      }
      SP?.Write(wa, 0, wa.Length);*/
    }
  }
}
