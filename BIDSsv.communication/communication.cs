using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
  public class communication : IBIDSsv
  {
    readonly int PortNum = 8032;
    string Addr = string.Empty;
    //byte[] MultiAddrByte = new byte[] { 127, 0, 0, 1 };
    public int Version { get; private set; } = 100;
    public string Name { get; private set; } = "communication";
    public bool IsDebug { get; set; } = false;

    UdpClient UC = null;
    Thread TD = null;
    Encoding Enc = Encoding.Default;
    bool IsLooping = true;

    public bool Connect(in string args)
    {
      throw new NotImplementedException();
    }

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    public void OnBSMDChanged(in BIDSSharedMemoryData data)
    {
    }

    public void OnOpenDChanged(in OpenD data)
    {
    }

    public void OnPanelDChanged(in int[] data)
    {
    }

    public void OnSoundDChanged(in int[] data)
    {

    }



    public void Print(in string data) => throw new NotImplementedException();

    public void Print(in byte[] data)
    {
      if (UC == null) return;
      byte[] ba = Common.BAtoBIDSBA(data);
      if (ba != null && ba.Length > 0) UC.Send(ba, ba.Length);
    }

    readonly string[] ArgInfo = new string[]
    {
      "Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
      "  -A or -Address : Set the Address to send to.  If you don't set this option, this program will run as a Reader.",
      "  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code."
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
