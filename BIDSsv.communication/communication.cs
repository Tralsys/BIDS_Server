using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
  public class communication : IBIDSsv
  {
    private int PortNum = 9032;
    IPAddress Addr = IPAddress.Any;
    public int Version { get; private set; } = 100;
    public string Name { get; private set; } = "communication";
    public bool IsDebug { get; set; } = false;
    bool IsWriteable = false;
    UdpClient UC = null;

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
                if (saa.Length > 1) IsWriteable = IPAddress.TryParse(saa[1], out Addr);
                break;
              case "Addr":
                if (saa.Length > 1) IsWriteable = IPAddress.TryParse(saa[1], out Addr);
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
            }
          }
        }
        catch (Exception e) { Console.WriteLine("Error has occured on " + Name); Console.WriteLine(e); }
      }
      try
      {
        UC = new UdpClient(new IPEndPoint(IPAddress.Any, PortNum));
        if (Addr != IPAddress.Any) UC?.Connect(Addr, PortNum);
      }
      catch (Exception e)
      {
        Console.WriteLine("{0} : {1}", Name, e);
        return false;
      }
      UC?.BeginReceive(ReceivedDoing, UC);
      return true;
    }
    SocketException sexc;
    private void ReceivedDoing(IAsyncResult ar)
    {
      UdpClient uc = (UdpClient)ar.AsyncState;
      IPEndPoint ipep = null;
      byte[] rba = new byte[] { 0x00 };
      try
      {
        rba = uc?.EndReceive(ar, ref ipep);
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

      if (rba.Length == (Common.ComStrSize + (Common.PSArrSize * 2)) && BitConverter.ToUInt32(rba, 0) == Common.CommunicationStructHeader)
      {
        int[] pda = new int[256];
        int[] sda = new int[256];
        var bsmd = Common.CommunicationBAGot(rba, out pda, out sda).ComStrtoBSMD();
        Common.BSMD = bsmd;
        Common.PD = new PanelD() { Panels = pda };
        Common.SD = new SoundD() { Sounds = sda };
      }

      uc?.BeginReceive(ReceivedDoing, uc);
    }

    public void Dispose()
    {
      var ipep = new IPEndPoint(IPAddress.Any, PortNum);
      //UC?.EndReceive(null, ref ipep);
      
      UC?.Close();
      UC?.Dispose();
    }
    int[] PDA = new int[256];
    int[] SDA = new int[256];
    int oldT = 0;
    public void OnBSMDChanged(in BIDSSharedMemoryData data)
    {
      if (!IsWriteable) return;
      UC?.Send(Common.CommunicationBAGet(Common.BSMDtoComStr(data, data.StateData.T - oldT), PDA, SDA), Common.ComStrSize + (Common.PSArrSize * 2));
      oldT = data.StateData.T;
    }

    public void OnOpenDChanged(in OpenD data) { }
    public void OnPanelDChanged(in int[] data)
    {
      if (!IsWriteable) return;
      Array.Copy(data, 0, PDA, 0, Math.Min(256, data.Length));
      if (data.Length < 256) Array.Clear(PDA, data.Length, 256 - data.Length);
    }
    public void OnSoundDChanged(in int[] data)
    {
      if (!IsWriteable) return;
      Array.Copy(data, 0, SDA, 0, Math.Min(256, data.Length));
      if (data.Length < 256) Array.Clear(SDA, data.Length, 256 - data.Length);
    }



    public void Print(in string data) => throw new NotImplementedException();

    public void Print(in byte[] data)
    {
      /*
      if (UC != null && data?.Length > 0)
      {
        if (BitConverter.ToUInt32(data, 0) == Common.CommunicationStructHeader)
        {
          UC.Send(data, data.Length);
        }
      }*/
    }

    readonly string[] ArgInfo = new string[]
    {
      "Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
      "  -A or -Address : Set the Address to send to.  If you don't set this option, this program will run as a Reader Only.",
      "  -P or -Port : Set the Sending Port.  If you don't set this option, this program uses \"9032\" as the Port Number.",
      "  -N or -Name : Set the Connection Name."
    };
    public void WriteHelp(in string args)
    {
      Console.WriteLine("BIDS Server Program Communication.dll Interface");
      Console.WriteLine("Version : " + Version.ToString());
      Console.WriteLine("Copyright (C) Tetsu Otter 2019");
      foreach (string s in ArgInfo) Console.WriteLine(s);
    }
  }
}
