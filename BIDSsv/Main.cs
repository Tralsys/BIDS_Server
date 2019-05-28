using System;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
  public class Main
  {
    static public readonly int Version = 202;
  }
  public class HandleCtrlEvArgs : EventArgs
  {
    public int? Reverser;
    public int? Power;
    public int? Brake;
  }
  public class KeyCtrlEvArgs : EventArgs
  {
    public bool?[] KeyState;
  }
  public interface IBIDSsv : IDisposable
  {
    event EventHandler<HandleCtrlEvArgs> HandleCtrl;
    event EventHandler<KeyCtrlEvArgs> KeyCtrl;
    event EventHandler BSMDChanged;
    event EventHandler OpenDChanged;
    event EventHandler PanelDChanged;
    event EventHandler SoundDChanged;

    int Version { get; }
    string Name { get; set; }
    bool IsDebug { get; set; }
    bool Connect(in string args);
    void OnBSMDChanged(in BIDSSharedMemoryData data);
    void OnOpenDChanged(in OpenD data);
    void OnPanelDChanged(in int[] data);
    void OnSoundDChanged(in int[] data);

    void WriteHelp(in string args);

  }
}
