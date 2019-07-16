using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
  public class udp : IBIDSsv
  {
    //use multicast
    //Refer : https://dobon.net/vb/dotnet/internet/udpclient.html
    readonly int PortNum = Common.DefPNum;
    const string MultiAddr = "239.0.8.32";
    readonly byte[] MultiAddrByte = new byte[] { 239, 0, 8, 32 };
    public int Version { get; private set; } = 202;
    public string Name { get; private set; } = "udp";
    public bool IsDebug { get; set; } = false;

    UdpClient UC = null;
    Thread TD = null;
    Encoding Enc = Encoding.Default;
    bool IsLooping = true;

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
            }
          }
        }
        catch (Exception e) { Console.WriteLine("Error has occured on " + Name); Console.WriteLine(e); }
      }
      try
      {
        UC = new UdpClient(PortNum);
        UC.JoinMulticastGroup(new IPAddress(MultiAddrByte));
      }
      catch (Exception e)
      {
        Console.WriteLine("{0} : {1}", Name, e);
        return false;
      }
      //TD = new Thread(ReadDoing);
      //TD.Start();
      UC?.BeginReceive(ReceivedDoing, UC);
      return true;
    }

    private void ReceivedDoing(IAsyncResult iar)
    {
      UdpClient uc = (UdpClient)iar.AsyncState;
      IPEndPoint ipep = null;
      byte[] rba = new byte[] { 0x00 };
      try
      {
        rba = Common.BIDSBAtoBA(uc?.EndReceive(iar, ref ipep));
      }
      catch (SocketException e)
      {
        if (!Equals(sexc.Message, e.Message)) Console.WriteLine("{0} : Receieve Error => {1}", Name, e);
        sexc = e;
      }
      catch (ObjectDisposedException)
      {
        Console.WriteLine("{0} (ReceivedDoing) : This connection is already closed.", Name);
        Common.Remove(Name);
        return;
      }

      if (rba != null) rba = Common.DataSelect(Name, rba, Enc);
      if (rba != null && rba.Length != 0) Print(rba);

      UC?.BeginReceive(ReceivedDoing, UC);
    }

    private void ReadDoing()
    {
      while (IsLooping)
      {
        byte[] BA = Common.DataSelect(Name, ReadByte(), Enc);
        if (BA != null && BA.Length != 0) Print(BA);
      }
    }

    public void Dispose()
    {
      IsLooping = false;
      
      //if (TD?.IsAlive == true && TD?.Join(5000) == false) Console.WriteLine("{0} : ReadThread is not closed.  It may cause some bugs.", Name);
      //TD?.Interrupt();
      UC?.DropMulticastGroup(new IPAddress(MultiAddrByte));
      UC?.Close();
      UC?.Dispose();
      UC = null;
    }

    public void OnBSMDChanged(in BIDSSharedMemoryData data) { }

    public void OnOpenDChanged(in OpenD data) { }

    public void OnPanelDChanged(in int[] data) { }

    public void OnSoundDChanged(in int[] data) { }

    SocketException sexc = null;
    Exception exc = null;
    string ReadString()
    {
      string s = Enc.GetString(ReadByte());
      if (IsDebug) Console.WriteLine("{0} << {1}", Name, s);
      return s;
    }
    byte[] ReadByte()
    {
      try
      {
        
        byte[] ba;
        IPEndPoint ipep = new IPEndPoint(new IPAddress(MultiAddrByte), PortNum);
        ba = Common.BIDSBAtoBA(UC?.ReceiveAsync().Result.Buffer);
        if (IsDebug) Console.WriteLine("{0} : Got from {1}:{2}\n>>{3}", Name, ipep.Address, ipep.Port, ba);
        return ba;
      }
      catch (SocketException e)
      {
        if (!Equals(e.Message, sexc.Message)) Console.WriteLine("{0} : {1}", Name, e);
        sexc = e;
      }
      catch (Exception e)
      {
        if (!Equals(e.Message, exc.Message)) Console.WriteLine("{0} : {1}", Name, e);
        exc = e;
      }
      return null;
    }

    public void Print(in string data)
    {
      if (IsDebug) Console.WriteLine("{0} >> {1}", Name, data);
      try
      {
        Print(Enc.GetBytes(data + (data.EndsWith("\n") ? string.Empty : "\n")));
      }
      catch(Exception e)
      {
        Console.WriteLine("{0} : {1}", Name, e);
      }
    }

    public void Print(in byte[] data)
    {
      if (UC == null) return;
      byte[] ba = Common.BAtoBIDSBA(data);
      if (ba != null && ba.Length > 0) UC?.BeginSend(ba, ba.Length, SendedDoing, UC);
    }

    private void SendedDoing(IAsyncResult ar)
    {
      UdpClient uc = (UdpClient)ar.AsyncState;
      try
      {
        uc?.EndSend(ar);
      }
      catch (SocketException e)
      {
        if (!Equals(sexc.Message, e.Message)) Console.WriteLine("{0} (SendedDoing) : Send Error => {1}", Name, e);
        sexc = e;
      }
      catch (ObjectDisposedException)
      {
        Console.WriteLine("{0} : This connection is already closed.", Name);
        Common.Remove(Name);
        return;
      }
    }

    readonly string[] ArgInfo = new string[]
    {
      "Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
      "  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -N or -Name : Set the Instance Name.  Default:\"udp\"  If you don't set this option, this program maybe cause some bugs."
    };
    public void WriteHelp(in string args)
    {
      Console.WriteLine("BIDS Server Program UDP Interface");
      Console.WriteLine("Version : " + Version.ToString());
      Console.WriteLine("Copyright (C) Tetsu Otter 2019");
      foreach (string s in ArgInfo) Console.WriteLine(s);
    }
  }
}
