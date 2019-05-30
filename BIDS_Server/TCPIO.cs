using System;
using TR.BIDSSMemLib;
using TR.BIDSsv;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace BIDS_Server
{
  class TCPIO : IBIDSsv
  {
    //Wait Port : 14147
    int PortNum = 14147;
    public int Version => 202;

    public string Name { get; private set; } = "tcpsv";
    public bool IsDebug { get; set; } = false;

    TcpListener TL = null;
    TcpClient TC = null;
    NetworkStream NS = null;
    Thread TD = null;

    Encoding Enc = Encoding.Default;
    bool IsLooping = true;
    int WTO = 1000;
    int RTO = 10000;

    public bool Connect(in string args)
    {
      string[] sa = args.Replace(" ", string.Empty).Split(new string[2] { "-", "/" }, StringSplitOptions.RemoveEmptyEntries);
      for (int i = 0; i < sa.Length; i++)
      {
        string[] saa = sa[i].Split(':');
        try
        {
          if (saa.Length > 0)
          {
            switch (saa[0])
            {
              case "E":
                switch (int.Parse(saa[1]))
                {
                  case 0:
                    Enc = Encoding.Default;
                    break;
                  case 1:
                    Enc = Encoding.ASCII;
                    break;
                  case 2:
                    Enc = Encoding.Unicode;
                    break;
                  case 3:
                    Enc = Encoding.UTF8;
                    break;
                  case 4:
                    Enc = Encoding.UTF32;
                    break;
                  default:
                    Enc = Encoding.Default;
                    break;
                }
                break;
              case "Encoding":
                switch (int.Parse(saa[1]))
                {
                  case 0:
                    Enc = Encoding.Default;
                    break;
                  case 1:
                    Enc = Encoding.ASCII;
                    break;
                  case 2:
                    Enc = Encoding.Unicode;
                    break;
                  case 3:
                    Enc = Encoding.UTF8;
                    break;
                  case 4:
                    Enc = Encoding.UTF32;
                    break;
                  default:
                    Enc = Encoding.Default;
                    break;
                }
                break;
              case "N":
                if (saa.Length > 1) Name = saa[1];
                break;
              case "Name":
                if (saa.Length > 1) Name = saa[1];
                break;
              case "P":
                if (saa.Length > 1) PortNum = int.Parse(saa[1]);
                break;
              case "Port":
                if (saa.Length > 1) PortNum = int.Parse(saa[1]);
                break;

              case "RTO":
                RTO = int.Parse(saa[1]);
                break;
              case "ReadTimeout":
                RTO = int.Parse(saa[1]);
                break;
              case "WTO":
                WTO = int.Parse(saa[1]);
                break;
              case "WriteTimeout":
                WTO = int.Parse(saa[1]);
                break;

            }
          }
        }catch(Exception e) { Console.WriteLine("Error has occured on " + Name);Console.WriteLine(e); }
      }
      TL = new TcpListener(IPAddress.Any, PortNum);
      TL.Start();
      Console.WriteLine(Name + " : " + "Connection Waiting Start => Addr:{0}, Port:{1}", ((IPEndPoint)TL.LocalEndpoint).Address, ((IPEndPoint)TL.LocalEndpoint).Port);
      TD = new Thread(ReadDoing);
      TD.Start();
      return true;
    }

    public void Dispose()
    {
      IsLooping = false;
      if (!TD.Join(5000)) Console.WriteLine(Name + " : " + "ReadThread is not closed.  It may cause some bugs.");
      NS?.Dispose();
      TC?.Dispose();
      TL?.Stop();
    }

    //https://dobon.net/vb/dotnet/internet/tcpclientserver.html
    void ReadDoing()
    {
      try
      {
        TC = TL.AcceptTcpClient();
        Console.WriteLine(Name + " : " + "Connected => Addr:{0}, Port:{1}", ((IPEndPoint)TC.Client.RemoteEndPoint).Address, ((IPEndPoint)TC.Client.RemoteEndPoint).Port);
        NS = TC?.GetStream();
      }
      catch(Exception e)
      {
        Console.WriteLine("In connection waiting process, An Error has occured on " + Name);
        Console.WriteLine(e);
      }
      (new Thread(() => {
        while (TC?.Connected == true) Thread.Sleep(1);
        IsLooping = false;
      })).Start();

      List<byte> RBytesLRec = new List<byte>();

      while (IsLooping)
      {
        if (TC?.Connected != true) continue;
        List<byte> RBytesL = RBytesLRec;
        try
        {
          while (NS?.DataAvailable == false && IsLooping) Thread.Sleep(1);
        }
        catch (Exception e)
        {
          Console.WriteLine("{0} : Error has occured at waiting process\n{1}", Name, e);
        }
        if (!IsLooping) continue;
        byte[] b = new byte[1];
        int nsreadr = 0;
        while (NS?.DataAvailable == true && !RBytesL.Contains((byte)'\n'))
        {
          b = new byte[1];
          nsreadr = NS.Read(b, 0, 1);
          if (nsreadr <= 0) break;
          RBytesL.Add(b[0]);
        }
        if (!RBytesL.Contains((byte)'\n'))
        {
          RBytesLRec.InsertRange(RBytesLRec.Count - 1, RBytesL);
          break;
        }
        string ReadData = Enc.GetString(RBytesL.ToArray());
        ReadData.TrimEnd('\n');
        if (ReadData.Contains("X")) Common.DataGot(ReadData);
        if (ReadData.StartsWith("TR")) Print(Common.DataSelectTR(Name, ReadData));
        else if (ReadData.StartsWith("TO")) Print(Common.DataSelectTO(ReadData));
      }
      NS?.Close();
      TC?.Close();

    }

    public void OnBSMDChanged(in BIDSSharedMemoryData data) { }
    public void OnOpenDChanged(in OpenD data) { }
    public void OnPanelDChanged(in int[] data) { }
    public void OnSoundDChanged(in int[] data) { }

    public void Print(in string data)
    {
      if (TC?.Connected != true || NS?.CanWrite != true) return;
      if (IsDebug) Console.Write("{0} >> {1}", Name, data);
      try
      {
        byte[] wbytes = Enc.GetBytes(data + (data.EndsWith("\n") ? string.Empty : "\n"));
        NS?.Write(wbytes, 0, wbytes.Length);
      }
      catch (Exception e)
      {
        Console.WriteLine("In Writing Process, An Error has occured on " + Name);
        Console.WriteLine(e);
      }
    }

    readonly string[] ArgInfo = new string[]
    {
      "Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
      "  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -N or -Name : Set the Instance Name.  Default:\"tcp\"  If you don't set this option, this program maybe cause some bugs.",
      "  -P or -PortName : Set PortNum.  Default:14147  Only Number is allowed in the setting.",
      "  -RTO or -ReadTimeout : Set the ReadTimeout Setting.  Default:10000",
      "  -WTO or -WriteTimeout : Set the WriteTimeout Setting.  Default:1000"
    };
    public void WriteHelp(in string args)
    {
      Console.WriteLine("BIDS Server Program TCP Interface");
      Console.WriteLine("Version : " + Version.ToString());
      Console.WriteLine("Copyright (C) Tetsu Otter 2019");
      foreach (string s in ArgInfo) Console.WriteLine(s);
    }
  }
}
