using System;
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

    public string Name { get; set; } = "serial";

    public event EventHandler<HandleCtrlEvArgs> HandleCtrl;
    public event EventHandler<KeyCtrlEvArgs> KeyCtrl;
    public event EventHandler BSMDChanged;
    public event EventHandler OpenDChanged;
    public event EventHandler PanelDChanged;
    public event EventHandler SoundDChanged;

    public bool Connect(in string args)
    {
      string[] sa = args.Replace(" ", string.Empty).Split(new string[2] { "-", "/" }, StringSplitOptions.RemoveEmptyEntries);



      return false;
    }

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    public void OnBSMDChanged(in BIDSSharedMemoryData data)
    {
      throw new NotImplementedException();
    }

    public void OnOpenDChanged(in OpenD data)
    {
      throw new NotImplementedException();
    }

    public void OnPanelDChanged(in int[] data)
    {
      throw new NotImplementedException();
    }

    public void OnSoundDChanged(in int[] data)
    {
      throw new NotImplementedException();
    }


    readonly string[] ArgInfo = new string[]
    {
      "Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
      "  -B or -BaudRate : Set BaudRate.  If you don\'t set this command, the BaudRate is set to 19200.",
      "  -DataBits : Set the DataBit Count Option.  Default:8  If you want More info about this argument, please read the source code.",
      "  -DTR : Set the RTS (Data Terminal Ready) Signal Option.  Default:1  0:Disabled, 1:Enabled",
      "  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -HandShake : Set the HandShake Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -P or -PortName : Set PortName.  You must set this option.  If you forgot setting this option, you can not open the port.  \"COM\" is unneeded in the Setting.  Only Number is allowed in the setting.",
      "  -Parity : Set the Parity Option.  Default:0  If you want More info about this argument, please read the source code.",
      "  -RTS : Set the RTS (Request to Send) Signal Option.  Default:1  0:Disabled, 1:Enabled",
      "  -StopBits : Set the StopBits Option.  Default:0  If you want More info about this argument, please read the source code."
    };
    public void WriteHelp(in string args)
    {
      Console.WriteLine("BIDS Server Program Serial Interface");
      Console.WriteLine("Version : " + Version.ToString());
      Console.WriteLine("Copyright © Tetsu Otter 2019");
      foreach (string s in ArgInfo) Console.WriteLine(s);

    }
  }
}
