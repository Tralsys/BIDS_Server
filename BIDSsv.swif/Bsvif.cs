using System;
using TR;
using TR.BIDSSMemLib;

namespace BIDSsv.swif
{
  public class Bsvif : IBIDSsv
  {
    public int Version => 202;

    public string Name => "swif";

    public bool IsDebug { get; set; }

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

    public void Print(in string data)
    {
      throw new NotImplementedException();
    }

    public void Print(in byte[] data)
    {
      throw new NotImplementedException();
    }

    public void WriteHelp(in string args)
    {
      throw new NotImplementedException();
    }
  }
}
