using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TR.BIDSsv
{
  [StructLayout(LayoutKind.Sequential)]
  public struct CommunicationFormat
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public int[] Panel;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public int[] Sound;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public readonly byte[] Foot;
  }
  internal class ComFormat
  {
    internal ComFormat()
    {

    }

  }
}
