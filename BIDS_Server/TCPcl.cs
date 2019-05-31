using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TR.BIDSSMemLib;
using TR.BIDSsv;

namespace BIDS_Server
{
  class TCPcl : IBIDSsv
  {
    public int Version { get; private set; } = 202;
    public string Name { get; private set; } = "tcpcl";
    public bool IsDebug { get; set; } = false;

    int PortNum = 14147;

    TcpClient TC = null;
    NetworkStream NS = null;
    Thread TD = null;
    string SvAddr = "127.0.0.1";
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
              case "A":
                if (saa.Length > 1) SvAddr = saa[1];
                break;
              case "Address":
                if (saa.Length > 1) SvAddr = saa[1];
                break;
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
        }
        catch (Exception e) { Console.WriteLine("Error has occured on " + Name); Console.WriteLine(e); }
      }

      TD = new Thread(LoopDoing);
      TD.Start();
      
      return true;
    }

    void LoopDoing()
    {
      try
      {
        TC = new TcpClient(SvAddr, PortNum);
        IPEndPoint rie = (IPEndPoint)TC.Client.RemoteEndPoint;
        IPEndPoint lie = (IPEndPoint)TC.Client.LocalEndPoint;
        Console.WriteLine("{0} : Connected to Addr:{1} Port:{2}, from Addr:{3} Port:{4}", Name, rie.Address, rie.Port, lie.Address, lie.Port);
        NS = TC?.GetStream();
        NS.ReadTimeout = RTO;
        NS.WriteTimeout = WTO;

        //Reconnect Process
        if (rie.Port == TCPIO.DefPNum)
        {
          Console.WriteLine("{0} : Reconnect Process Start.", Name);
          try
          {
            Print("TRV" + Version.ToString() + "\n");
            while (IsLooping && TC?.Connected == true && NS?.DataAvailable == false) Thread.Sleep(1);
            if (IsLooping)
            {
              string gs = Read();
              if (gs?.StartsWith("TRV") != true)
              {
                IsLooping = false;
                throw new Exception("Version Check Process Failed.");
              }
              else
              {
                string[] gsa = gs.Split(new string[] { "PN" }, StringSplitOptions.RemoveEmptyEntries);
                int v = int.Parse(gsa[0].Replace("TRV", string.Empty));
                Version = v < Version ? v : Version;
                if (gsa.Length >= 2)
                {
                  PortNum = int.Parse(gsa[1]);
                  if (PortNum != rie.Port)
                  {
                    NS?.Close();
                    NS?.Dispose();
                    TC?.Close();
                    TC?.Dispose();
                    TC = new TcpClient(SvAddr, PortNum);
                    NS = TC?.GetStream();
                    NS.ReadTimeout = RTO;
                    NS.WriteTimeout = WTO;
                  }
                }
              }
            }
            IPEndPoint rier = (IPEndPoint)TC.Client.RemoteEndPoint;
            IPEndPoint lier = (IPEndPoint)TC.Client.LocalEndPoint;
            Console.WriteLine("{0} : Reconnect Success to Addr={1} Port={2}, from Addr={3} Port={4}", Name, rier.Address, rier.Port, lier.Address, lier.Port);
          }
          catch (Exception e)
          {
            Console.WriteLine("{0} : ReConnect Process Failed.\n{1}", Name, e);
          }
        }
        //Reconnect Process End
      }
      catch (Exception e)
      {
        Console.WriteLine(Name + " : TcpClient Open Failed");
        Console.WriteLine(e);

        Common.Remove(Name);
        return;
      }


      (new Thread(() =>
      {
        while (TC?.Connected == true) Thread.Sleep(1);
        IsLooping = false;
      })).Start();


      while (IsLooping)
      {
        if (TC?.Connected != true) continue;
        Print("TRID0\n");
        if (NS?.CanRead != true) continue;
        string ReadData = Read();
        if (ReadData.Contains("X")) Common.DataGot(ReadData);
        else if (ReadData.StartsWith("TR")) Print(Common.DataSelectTR(Name, ReadData));
        else if (ReadData.StartsWith("TO")) Print(Common.DataSelectTO(ReadData));

      }
      NS?.Close();
      TC?.Close();
    }

    public void Dispose()
    {
      IsLooping = false;
      if (TD?.IsAlive == true && TD?.Join(5000) == false) Console.WriteLine(Name + " : Thread Closing Failed");
      NS?.Dispose();
      TC?.Dispose();
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
        NS.WriteTimeout = WTO;
        NS.Write(wbytes, 0, wbytes.Length);
      }
      catch (Exception e)
      {
        Console.WriteLine("In Writing Process, An Error has occured on " + Name);
        Console.WriteLine(e);
      }
    }

    List<byte> RBytesLRec = new List<byte>();
    string Read()
    {
      List<byte> RBytesL = RBytesLRec;
      try
      {
        while (NS?.DataAvailable == false && IsLooping) Thread.Sleep(1);
      }
      catch (Exception e)
      {
        Console.WriteLine("{0} : Error has occured at waiting process\n{1}", Name, e);
      }
      if (!IsLooping) return string.Empty;
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
        if (RBytesLRec.Count == 0) RBytesLRec = RBytesL;
        else RBytesLRec.InsertRange(RBytesLRec.Count - 1, RBytesL);
        return string.Empty;
      }

      return Enc.GetString(RBytesL.ToArray()).Replace("\n", string.Empty);
    }

    readonly string[] ArgInfo = new string[]
    {
      "Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
      "  -A or -Address : Set the ipv4 address of Server.  Default:127.0.0.1",
      "  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -N or -Name : Set the Instance Name.  Default:\"tcp\"  If you don't set this option, this program maybe cause some bugs.",
      "  -P or -PortName : Set Server's PortNumber.  NOT CLIENT's PORTNUM.  Default:14147  Only Number is allowed in the setting.",
      "  -RTO or -ReadTimeout : Set the ReadTimeout Setting.  Default:10000",
      "  -WTO or -WriteTimeout : Set the WriteTimeout Setting.  Default:1000"
    };
    public void WriteHelp(in string args)
    {
      Console.WriteLine("BIDS Server Program TCP Interface (Client Side)");
      Console.WriteLine("Version : " + Version.ToString());
      Console.WriteLine("Copyright (C) Tetsu Otter 2019");
      foreach (string s in ArgInfo) Console.WriteLine(s);
    }
  }
}
