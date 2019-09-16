using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
  [StructLayout(LayoutKind.Sequential)]
  public struct CommunicationStruct
  {
    public UInt32 Header;
    public double Location;
    public float speed;
    public int time;
    public float BCPres;
    public float MRPres;
    public float ERPres;
    public float BPPres;
    public float SAPPres;
    public float Current;
    public int isDoorClosed;
    public float SpeedAbs;
    public int deltaT;
    public int BrakeHPos;
    public int PowerHPos;
    public int ReverHPos;
    public int ConstSPD;
    public int BrakeNNum;
    public int PowerNNum;
    public int ATSCheckPos;
    public int B67NNum;
    public int CarCount;
    public int EBPos;
    public int MaxServiceBrakeNNum;
  }

  static public partial class Common
  {
    public const UInt32 CommunicationStructHeader = 0xfefef0f0;

    static public readonly int ComStrSize = Marshal.SizeOf(new CommunicationStruct());
    public const int PSArrSize = 256 * sizeof(int);

    static public CommunicationStruct CommunicationBAGot(byte[] ba, out int[] PA, out int[] SA)
    {
      PA = new int[256];
      SA = new int[256];

      IntPtr ip = Marshal.AllocHGlobal(ComStrSize);
      Marshal.Copy(ba, 0, ip, ComStrSize);
      var cs = (CommunicationStruct)Marshal.PtrToStructure(ip, typeof(CommunicationStruct));
      Marshal.FreeHGlobal(ip);

      ip = Marshal.AllocHGlobal(PSArrSize);
      Marshal.Copy(ba, ComStrSize, ip, PSArrSize);
      Marshal.Copy(ip, PA, 0, 256);
      Marshal.FreeHGlobal(ip);
      ip = Marshal.AllocHGlobal(PSArrSize);
      Marshal.Copy(ba, ComStrSize + PSArrSize, ip, PSArrSize);
      Marshal.Copy(ip, SA, 0, 256);
      Marshal.FreeHGlobal(ip);

      return cs;
    }
    /// <summary>
    /// BIDS Data to Communication.dll data
    /// </summary>
    /// <param name="BSMD">BIDS SharedMemory Data</param>
    /// <param name="PD">Panel Data</param>
    /// <param name="SD">Sound Data</param>
    /// <param name="deltaT">delta T Data</param>
    /// <returns>Communication.dll Byte Array</returns>
    static public byte[] CommunicationBAGet(CommunicationStruct cs, int[] PD, int[] SD)
    {
      int[] pda = new int[256];
      int[] sda = new int[256];
      Array.Copy(PD, 0, pda, 0, Math.Min(pda.Length, PD.Length));
      Array.Copy(SD, 0, sda, 0, Math.Min(sda.Length, SD.Length));

      byte[] ba = new byte[ComStrSize + (PSArrSize * 2)];

      IntPtr ip = Marshal.AllocHGlobal(ComStrSize);
      Marshal.StructureToPtr(cs, ip, false);
      Marshal.Copy(ip, ba, 0, ComStrSize);
      Marshal.FreeHGlobal(ip);

      ip = Marshal.AllocHGlobal(PSArrSize);
      Marshal.Copy(pda, 0, ip, 256);
      Marshal.Copy(ip, ba, ComStrSize, PSArrSize);
      Marshal.FreeHGlobal(ip);
      ip = Marshal.AllocHGlobal(PSArrSize);
      Marshal.Copy(sda, 0, ip, 256);
      Marshal.Copy(ip, ba, ComStrSize + PSArrSize, PSArrSize);
      Marshal.FreeHGlobal(ip);

      return ba;
    }

    static public BIDSSharedMemoryData ComStrtoBSMD(this CommunicationStruct cs) => cs.ComStrtoBSMD(out _);
    static public BIDSSharedMemoryData ComStrtoBSMD(this CommunicationStruct cs, out int deltaT)
    {
      deltaT = cs.deltaT;
      return new BIDSSharedMemoryData()
      {
        IsEnabled = true,
        VersionNum = 202,
        IsDoorClosed = cs.isDoorClosed == 1,
        HandleData = new Hand()
        {
          B = cs.BrakeHPos,
          P = cs.PowerHPos,
          R = cs.ReverHPos,
          C = cs.ConstSPD
        },
        SpecData = new Spec()
        {
          A = cs.ATSCheckPos,
          B = cs.BrakeNNum,
          C = cs.CarCount,
          J = cs.B67NNum,
          P = cs.PowerNNum
        },
        StateData = new State()
        {
          BC = cs.BCPres,
          BP = cs.BPPres,
          ER = cs.ERPres,
          I = cs.Current,
          MR = cs.MRPres,
          SAP = cs.SAPPres,
          T = cs.time,
          V = cs.speed,
          Z = cs.Location
        }
      };
    }
    static public CommunicationStruct BSMDtoComStr(this BIDSSharedMemoryData bsmd, int deltaT) => new CommunicationStruct()
    {
      Header = CommunicationStructHeader,
      deltaT = deltaT,
      ATSCheckPos = bsmd.SpecData.A,
      B67NNum = bsmd.SpecData.J,
      BCPres = bsmd.StateData.BC,
      BPPres = bsmd.StateData.BP,
      BrakeHPos = bsmd.HandleData.B,
      BrakeNNum = bsmd.SpecData.B,
      CarCount = bsmd.SpecData.C,
      ConstSPD = bsmd.HandleData.C,
      Current = bsmd.StateData.I,
      EBPos = bsmd.SpecData.B,
      ERPres = bsmd.StateData.ER,
      isDoorClosed = bsmd.IsDoorClosed ? 1 : 0,
      Location = bsmd.StateData.Z,
      MaxServiceBrakeNNum = bsmd.SpecData.J,
      MRPres = bsmd.StateData.MR,
      PowerHPos = bsmd.HandleData.P,
      PowerNNum = bsmd.SpecData.P,
      ReverHPos = bsmd.HandleData.R,
      SAPPres = bsmd.StateData.SAP,
      speed = bsmd.StateData.V,
      SpeedAbs = Math.Abs(bsmd.StateData.V),
      time = bsmd.StateData.T
    };
  }
}
