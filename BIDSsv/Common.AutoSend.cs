using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
  //AutoSendに関連する実装

  static public partial class Common
  {
    static public event EventHandler<SMemLib.BSMDChangedEArgs> BSMDChanged
    {
      add => SMemLib.BIDSSMemChanged += value;
      remove => SMemLib.BIDSSMemChanged -= value;
    }
    static public event EventHandler<SMemLib.OpenDChangedEArgs> OpenDChanged
    {
      add => SMemLib.OpenDChanged += value;
      remove => SMemLib.OpenDChanged -= value;
    }
    static public event EventHandler<SMemLib.ArrayDChangedEArgs> PanelDChanged
    {
      add => SMemLib.PanelDChanged += value;
      remove => SMemLib.PanelDChanged -= value;
    }
    static public event EventHandler<SMemLib.ArrayDChangedEArgs> SoundDChanged
    {
      add => SMemLib.SoundDChanged += value;
      remove => SMemLib.SoundDChanged -= value;
    }

    public class AutoSendSetting
    {
      public const uint HeaderBasicConst = 0x74726201;
      public const uint BasicConstSize = 20;
      public const uint HeaderBasicCommon = 0x74726202;
      public const uint BasicCommonSize = 53;
      public const uint HeaderBasicBVE5 = 0x74726203;
      public const uint HeaderBasicOBVE = 0x74726204;
      public const uint HeaderBasicHandle = 0x74726205;
      public const uint BasicHandleSize = 20;
      public const uint HeaderPanel = 0x74727000;
      public const uint PanelSize = 129 * sizeof(int);
      public const uint HeaderSound = 0x74727300;
      public const uint SoundSize = PanelSize;

      public static bool BasicConstAS = false;
      public static bool BasicCommonAS = false;
      public static bool BasicBVE5AS = false;
      public static bool BasicOBVEAS = false;
      public static bool BasicHandleAS = false;
      public static bool BasicPanelAS = false;
      public static bool BasicSoundAS = false;

      static internal byte[] BasicConst(in Spec s, in OpenD o)
      {
        byte[] ba = new byte[BasicConstSize];
        int i = 0;
        Array.Copy(UFunc.GetBytes((int)HeaderBasicConst), 0, ba, i, sizeof(int)); i += sizeof(int);
        Array.Copy(UFunc.GetBytes((short)202), 0, ba, i, sizeof(short)); i += sizeof(short);
        Array.Copy(UFunc.GetBytes((short)s.B), 0, ba, i, sizeof(short)); i += sizeof(short);
        Array.Copy(UFunc.GetBytes((short)s.P), 0, ba, i, sizeof(short)); i += sizeof(short);
        Array.Copy(UFunc.GetBytes((short)s.A), 0, ba, i, sizeof(short)); i += sizeof(short);
        Array.Copy(UFunc.GetBytes((short)s.J), 0, ba, i, sizeof(short)); i += sizeof(short);
        Array.Copy(UFunc.GetBytes((short)s.C), 0, ba, i, sizeof(short)); i += sizeof(short);
        Array.Copy(UFunc.GetBytes((short)o.SelfBCount), 0, ba, i, sizeof(short));
        return ba;
      }
      static internal byte[] BasicCommon(in State s, byte DoorState, int SigNum = 0)
      {
        byte[] ba = new byte[BasicCommonSize];
        int i = 0;
        Array.Copy(UFunc.GetBytes((int)HeaderBasicCommon), 0, ba, i, sizeof(int)); i += sizeof(int);
        Array.Copy(UFunc.GetBytes((double)s.Z), 0, ba, i, sizeof(double)); i += sizeof(double);
        Array.Copy(UFunc.GetBytes((float)s.V), 0, ba, i, sizeof(float)); i += sizeof(float);
        Array.Copy(UFunc.GetBytes((float)s.I), 0, ba, i, sizeof(float)); i += sizeof(float);
        Array.Copy(UFunc.GetBytes((float)0.0), 0, ba, i, sizeof(float)); i += sizeof(float);
        Array.Copy(UFunc.GetBytes((int)s.T), 0, ba, i, sizeof(int)); i += sizeof(int);
        Array.Copy(UFunc.GetBytes((float)s.BC), 0, ba, i, sizeof(float)); i += sizeof(float);
        Array.Copy(UFunc.GetBytes((float)s.MR), 0, ba, i, sizeof(float)); i += sizeof(float);
        Array.Copy(UFunc.GetBytes((float)s.ER), 0, ba, i, sizeof(float)); i += sizeof(float);
        Array.Copy(UFunc.GetBytes((float)s.BP), 0, ba, i, sizeof(float)); i += sizeof(float);
        Array.Copy(UFunc.GetBytes((float)s.SAP), 0, ba, i, sizeof(float)); i += sizeof(float);
        Array.Copy(UFunc.GetBytes((int)SigNum), 0, ba, i, sizeof(int)); i += sizeof(int);
        ba[i] = DoorState;
        return ba;
      }
      static internal byte[] BasicBVE5(object o)
      {
        return null;
      }
      static internal byte[] BasicOBVE(OpenD o)
      {
        byte[] ba = new byte[Marshal.SizeOf(new OpenD()) + 4];
        Array.Copy(UFunc.GetBytes((int)HeaderBasicOBVE), 0, ba, 0, sizeof(int));
        IntPtr ip = new IntPtr();
        Marshal.StructureToPtr(o, ip, true);
        Marshal.Copy(ip, ba, 4, ba.Length);
        return ba;
      }
      static internal byte[] BasicHandle(Hand h, int SelfB)
      {
        byte[] ba = new byte[BasicHandleSize];
        int i = 0;
        Array.Copy(UFunc.GetBytes((int)HeaderBasicHandle), 0, ba, i, sizeof(int)); i += sizeof(int);
        Array.Copy(UFunc.GetBytes((int)h.P), 0, ba, i, sizeof(int)); i += sizeof(int);
        Array.Copy(UFunc.GetBytes((int)h.B), 0, ba, i, sizeof(int)); i += sizeof(int);
        Array.Copy(UFunc.GetBytes((int)h.R), 0, ba, i, sizeof(int)); i += sizeof(int);
        Array.Copy(UFunc.GetBytes((int)SelfB), 0, ba, i, sizeof(int));
        return ba;
      }
      /// <summary>
      /// 
      /// </summary>
      /// <param name="a">Source Array</param>
      /// <param name="SttInd">Start Index</param>
      /// <returns>Result Array</returns>
      static internal byte[] BasicPanel(int[] a, int SttInd)
      {
        byte[] ba = new byte[PanelSize];
        Array.Copy(HeaderPanel.GetBytes(), 0, ba, 0, sizeof(uint));
        ba[3] = (byte)(SttInd / 128);

        Parallel.For(0, 128, (i) =>
          {
            if (a.Length <= (SttInd * 128) + i) return;
            Array.Copy(a[i].GetBytes(), 0, ba, (i + 1) * sizeof(int), sizeof(int));
          });

        return ba;
      }
      static internal byte[] BasicSound(int[] a, int SttInd)
      {
        byte[] ba = new byte[SoundSize];

        Parallel.For(0, 128, (i) =>
        {
          if (a.Length <= (SttInd * 128) + i) return;
          Array.Copy(a[i].GetBytes(), 0, ba, (i + 1) * sizeof(int), sizeof(int));
        });

        Array.Copy(HeaderSound.GetBytes(), 0, ba, 0, sizeof(uint));
        ba[3] = (byte)(SttInd / 128);
        return ba;
      }
    }

    static private void ASPtr(byte[] ba)
    {
      if (ba != null && ba.Length > 0 && svlist?.Count > 0)
        Parallel.For(0, svlist.Count, (i) => svlist[i]?.Print(ba));
    }

    static private ASList PDAutoList = new ASList();
    static private ASList SDAutoList = new ASList();
    static private ASList AutoNumL = new ASList();

    private static void Common_SoundDChanged(object sender, SMemLib.ArrayDChangedEArgs e)
    {
      if (!IsStarted) return;
      if (svlist?.Count > 0)
      {
        Parallel.For(0, svlist.Count, (i) => svlist[i].OnSoundDChanged(in e.NewArray));

        #region Byte Array Auto Sender
        int al = Math.Max(e.OldArray.Length, e.NewArray.Length);
        if (al % 128 != 0) al = ((int)Math.Floor((double)al / 128) + 1) * 128;

        int[] oa = new int[al];
        int[] na = new int[al];
        Array.Copy(e.OldArray, oa, e.OldArray.Length);
        Array.Copy(e.NewArray, na, e.NewArray.Length);

        for (int i = 0; i < al; i += 128)
          if (!(oa, na).ArrayEqual(i, i, 128)) ASPtr(AutoSendSetting.BasicSound(na, i));
        
        #endregion

        if (SDAutoList?.Count > 0)
          Parallel.For(0, Math.Max(e.OldArray.Length, e.NewArray.Length), (i) =>
          {
            if (SDAutoList.DNums.Contains(i))
            {
              int? Num = null;
              if (e.OldArray.Length <= i) Num = e.NewArray[i];
              else if (e.NewArray.Length > i && e.OldArray[i] != e.NewArray[i]) Num = e.NewArray[i];

              if (Num != null)
              {
                Parallel.For(0, SDAutoList.Count, (s) =>
                {
                  if (SDAutoList.DNums[s] == i)
                    SDAutoList.SvList[s].Print("TRIS" + i.ToString() + "X" + Num.ToString());
                });
              }
            }
          });
      }

    }
    private static void Common_PanelDChanged(object sender, SMemLib.ArrayDChangedEArgs e)
    {
      if (!IsStarted) return;
      if (svlist?.Count > 0)
      {
        Parallel.For(0, svlist.Count, (i) => svlist[i].OnPanelDChanged(in e.NewArray));

        #region Byte Array Auto Sender
        int al = Math.Max(e.OldArray.Length, e.NewArray.Length);
        if (al % 128 != 0) al = ((int)Math.Floor((double)al / 128) + 1) * 128;
        int[] oa = new int[al];
        int[] na = new int[al];
        Array.Copy(e.OldArray, oa, e.OldArray.Length);
        Array.Copy(e.NewArray, na, e.NewArray.Length);
        for (int i = 0; i < al; i += 128)
          if (!(oa, na).ArrayEqual(i, i, 128)) ASPtr(AutoSendSetting.BasicPanel(na, i));
        #endregion


        if (PDAutoList?.Count > 0)
          Parallel.For(0, Math.Max(e.OldArray.Length, e.NewArray.Length), (i) =>
          {
            if (PDAutoList.DNums.Contains(i))
            {
              int? Num = null;
              if (e.OldArray.Length <= i) Num = e.NewArray[i];
              else if (e.NewArray.Length > i && e.OldArray[i] != e.NewArray[i]) Num = e.NewArray[i];

              if (Num != null)
              {
                Parallel.For(0, PDAutoList.Count, (s) =>
                {
                  if (PDAutoList.DNums[s] == i)
                    PDAutoList.SvList[s].Print("TRIP" + i.ToString() + "X" + Num.ToString());
                });
              }
            }
          });
      }
    }
    private static void Common_OpenDChanged(object sender, SMemLib.OpenDChangedEArgs e)
    {
      if (!IsStarted) return;
      if (svlist?.Count > 0)
      {
        if (!Equals(e.OldData.SelfBPosition, e.NewData.SelfBPosition))
          ASPtr(AutoSendSetting.BasicHandle(BSMD.HandleData, e.NewData.SelfBPosition));

        Parallel.For(0, svlist.Count, (i) => svlist[i].OnOpenDChanged(in e.NewData));
      }
    }
    private static void Common_BSMDChanged(object sender, SMemLib.BSMDChangedEArgs e)
    {
      if (!IsStarted) return;
      if (svlist?.Count > 0)
      {
        Parallel.For(0, svlist.Count, (i) => svlist[i].OnBSMDChanged(in e.NewData));

        if (!Equals(e.OldData.SpecData, e.NewData.SpecData))
          ASPtr(AutoSendSetting.BasicConst(e.NewData.SpecData, OD));
        if (!Equals(e.OldData.StateData, e.NewData.StateData) || e.OldData.IsDoorClosed != e.NewData.IsDoorClosed)
          ASPtr(AutoSendSetting.BasicCommon(e.NewData.StateData, (byte)(e.NewData.IsDoorClosed ? 1 : 0)));
        if (!Equals(e.NewData.HandleData, e.OldData.HandleData))
          ASPtr(AutoSendSetting.BasicHandle(e.NewData.HandleData, OD.SelfBPosition));

        if (AutoNumL?.Count > 0)
        {
          Parallel.For(0, AutoNumL.Count, (ind) =>
          {
            char dtyp = AutoNumL.DTypes[ind];
            int dnum = AutoNumL.DNums[ind];
            IBIDSsv sv = AutoNumL.SvList[ind];

            if (sv?.IsDisposed != false) return;//null=>return, Disposed=T:return, Disposed=F:next

            string data = string.Empty;
            try
            {
              data = Get_TRI_Data(dtyp, dnum) ?? string.Empty;
            }
            catch (BIDSErrException) { return; }
            catch (Exception e) { Console.WriteLine("Common.AutoSend.Common_BSMDChanged : {0}", e); }

            TimeSpan ots = TimeSpan.FromMilliseconds(e.OldData.StateData.T);
            TimeSpan nts = TimeSpan.FromMilliseconds(e.NewData.StateData.T);

            if (dtyp switch
            {
              ConstVals.DTYPE_CONSTD => (ConstVals.DNums.ConstD)dnum switch
              {
                ConstVals.DNums.ConstD.AllData => Equals(e.OldData.SpecData, e.NewData.SpecData),
                ConstVals.DNums.ConstD.ATSCheckPos => e.OldData.SpecData.A == e.NewData.SpecData.A,
                ConstVals.DNums.ConstD.B67_Pos => e.OldData.SpecData.J == e.NewData.SpecData.J,
                ConstVals.DNums.ConstD.Brake_Count => e.OldData.SpecData.B == e.NewData.SpecData.B,
                ConstVals.DNums.ConstD.Car_Count => e.OldData.SpecData.C == e.NewData.SpecData.C,
                ConstVals.DNums.ConstD.Power_Count => e.OldData.SpecData.P == e.NewData.SpecData.P,
                _ => null
              },

              ConstVals.DTYPE_DOOR => e.OldData.IsDoorClosed == e.NewData.IsDoorClosed,

              ConstVals.DTYPE_ELAPD => (ConstVals.DNums.ElapD)dnum switch
              {
                ConstVals.DNums.ElapD.AllData => Equals(e.OldData.StateData, e.NewData.StateData),
                ConstVals.DNums.ElapD.BC_Pres => e.OldData.StateData.BC == e.NewData.StateData.BC,
                ConstVals.DNums.ElapD.BP_Pres => e.OldData.StateData.BP == e.NewData.StateData.BP,
                ConstVals.DNums.ElapD.Current => e.OldData.StateData.I == e.NewData.StateData.I,
                ConstVals.DNums.ElapD.Distance => e.OldData.StateData.Z == e.NewData.StateData.Z,
                ConstVals.DNums.ElapD.ER_Pres => e.OldData.StateData.ER == e.NewData.StateData.ER,
                ConstVals.DNums.ElapD.MR_Pres => e.OldData.StateData.MR == e.NewData.StateData.MR,
                ConstVals.DNums.ElapD.Pressures => UFunc.State_Pressure_IsSame(e.OldData.StateData, e.NewData.StateData),
                ConstVals.DNums.ElapD.SAP_Pres => e.OldData.StateData.SAP == e.NewData.StateData.SAP,
                ConstVals.DNums.ElapD.Speed => e.OldData.StateData.V == e.NewData.StateData.V,
                ConstVals.DNums.ElapD.Time => e.OldData.StateData.T == e.NewData.StateData.T,
                ConstVals.DNums.ElapD.Time_HMSms => e.OldData.StateData.T == e.NewData.StateData.T,
                ConstVals.DNums.ElapD.TIME_Hour => ots.Hours == nts.Hours,
                ConstVals.DNums.ElapD.TIME_Min => ots.Minutes == nts.Minutes,
                ConstVals.DNums.ElapD.TIME_MSec => ots.Milliseconds == nts.Milliseconds,
                ConstVals.DNums.ElapD.TIME_Sec => ots.Seconds == nts.Seconds,
                ConstVals.DNums.ElapD.Voltage => null,
                _ => null
              },

              ConstVals.DTYPE_HANDPOS => (ConstVals.DNums.HandPos)dnum switch
              {
                ConstVals.DNums.HandPos.AllData => Equals(e.OldData.HandleData, e.NewData.HandleData),
                ConstVals.DNums.HandPos.Brake => e.OldData.HandleData.B == e.NewData.HandleData.B,
                ConstVals.DNums.HandPos.ConstSpd => e.OldData.HandleData.C == e.NewData.HandleData.C,
                ConstVals.DNums.HandPos.Power => e.OldData.HandleData.P == e.NewData.HandleData.P,
                ConstVals.DNums.HandPos.Reverser => e.OldData.HandleData.R == e.NewData.HandleData.R,
                ConstVals.DNums.HandPos.SelfB => null,
                _ => null
              },

              _ => null
            }
            == false) sv?.Print(UFunc.BIDSCMDMaker(ConstVals.CMD_INFOREQ, dtyp, dnum, data));
          });
        }
      }
    }

  }

}
