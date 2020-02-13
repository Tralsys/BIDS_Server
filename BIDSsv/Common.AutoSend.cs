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
        Array.Copy(UFunc.GetBytes((float)0), 0, ba, i, sizeof(float)); i += sizeof(float);
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

    const int OpenDBias = 1000000;
    const int ElapDBias = 100000;
    const int DoorDBias = 10000;
    const int HandDBias = 1000;

    static private void ASPtr(byte[] ba)
    {
      if (ba != null && ba.Length > 0)
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
        {
          bool IsNEqual = false;
          Parallel.For(0, 128, (j) =>
          {
            if (!IsNEqual) IsNEqual = oa[i + j] == na[i + j];
          });
          if (IsNEqual) ASPtr(AutoSendSetting.BasicSound(na, i));
        }
        #endregion

        if (SDAutoList?.Count > 0)
          Parallel.For(0, Math.Max(e.OldArray.Length, e.NewArray.Length), (i) =>
          {
            if (SDAutoList.Values.Contains(i))
            {
              int? Num = null;
              if (e.OldArray.Length <= i) Num = e.NewArray[i];
              else if (e.NewArray.Length > i && e.OldArray[i] != e.NewArray[i]) Num = e.NewArray[i];

              if (Num != null)
              {
                Parallel.For(0, svlist.Count, (s) =>
                {
                  if (SDAutoList.Contains(new KeyValuePair<string, int>(svlist[s].Name, i)))
                    svlist[s].Print("TRIS" + i.ToString() + "X" + Num.ToString());
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
        {
          bool IsNEqual = false;
          /*Parallel.For(0, 128, (j) =>
          {
            if (!IsNEqual) IsNEqual = oa[i + j] == na[i + j];
          });
          if (IsNEqual) ASPtr(AutoSendSetting.BasicPanel(na, i));*/
          if (na.Skip(i).Take(128).SequenceEqual(oa.Skip(i).Take(128))) ASPtr(AutoSendSetting.BasicPanel(na, i));
        }
        #endregion


        if (PDAutoList?.Count > 0)
          Parallel.For(0, Math.Max(e.OldArray.Length, e.NewArray.Length), (i) =>
          {
            if (PDAutoList.Values.Contains(i))
            {
              int? Num = null;
              if (e.OldArray.Length <= i) Num = e.NewArray[i];
              else if (e.NewArray.Length > i && e.OldArray[i] != e.NewArray[i]) Num = e.NewArray[i];

              if (Num != null)
              {
                Parallel.For(0, svlist.Count, (s) =>
                {
                  if (PDAutoList.Contains(new KeyValuePair<string, int>(svlist[s].Name, i)))
                    svlist[s].Print("TRIP" + i.ToString() + "X" + Num.ToString());
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
          bool IsDClsdo = e.OldData.IsDoorClosed;
          bool IsDClsd = e.NewData.IsDoorClosed;
          Spec osp = e.OldData.SpecData;
          Spec nsp = e.NewData.SpecData;
          State ost = e.OldData.StateData;
          State nst = e.NewData.StateData;
          Hand oh = e.OldData.HandleData;
          Hand nh = e.NewData.HandleData;
          TimeSpan ots = TimeSpan.FromMilliseconds(e.OldData.StateData.T);
          TimeSpan nts = TimeSpan.FromMilliseconds(e.NewData.StateData.T);
          ICollection<int> IC = AutoNumL.Values;
          ICollection<int> ICR = default;

          Parallel.For(0, IC.Count, (ind) =>
          {
            int i = IC.ElementAt(ind);
            if (!ICR.Contains(i))
            {
              ICR.Add(i);

              string WriteStr = string.Empty;
              string chr = string.Empty;
              int num = 0;

              if (OpenDBias > i && i >= ElapDBias)
              {
                switch (i - ElapDBias)
                {
                  case 0: WriteStr = UFunc.Comp(ost.Z, nst.Z); break;
                  case 1: WriteStr = UFunc.Comp(ost.V, nst.V); break;
                  case 2: WriteStr = UFunc.Comp(ost.T, nst.T); break;
                  case 3: WriteStr = UFunc.Comp(ost.BC, nst.BC); break;
                  case 4: WriteStr = UFunc.Comp(ost.MR, nst.MR); break;
                  case 5: WriteStr = UFunc.Comp(ost.ER, nst.ER); break;
                  case 6: WriteStr = UFunc.Comp(ost.BP, nst.BP); break;
                  case 7: WriteStr = UFunc.Comp(ost.SAP, nst.SAP); break;
                  case 8: WriteStr = UFunc.Comp(ost.I, nst.I); break;
                  //case 9: WriteStr = UFunc.Comp(ost.Z, nst.Z); break;
                  case 10: WriteStr = UFunc.Comp(ots.Hours, nts.Hours); break;
                  case 11: WriteStr = UFunc.Comp(ots.Minutes, nts.Minutes); break;
                  case 12: WriteStr = UFunc.Comp(ots.Seconds, nts.Seconds); break;
                  case 13: WriteStr = UFunc.Comp(ots.Milliseconds, nts.Milliseconds); break;
                }
                (chr, num) = ("E", i - ElapDBias);
              }
              else if (i >= DoorDBias)
              {
                switch (i - DoorDBias)
                {
                  case 0: WriteStr = UFunc.Comp(IsDClsdo ? 1 : 0, IsDClsd ? 1 : 0); break;
                }
                (chr, num) = ("D", i - DoorDBias);
              }
              else if (i >= HandDBias)
              {
                switch (i - HandDBias)
                {
                  case 0: WriteStr = UFunc.Comp(oh.B, nh.B); break;
                  case 1: WriteStr = UFunc.Comp(oh.P, nh.P); break;
                  case 2: WriteStr = UFunc.Comp(oh.R, nh.R); break;
                  case 3: WriteStr = UFunc.Comp(oh.C, nh.C); break;
                }
                (chr, num) = ("H", i - HandDBias);
              }
              else if (OpenDBias > i)
              {
                switch (i)
                {
                  case 0: WriteStr = UFunc.Comp(osp.B, nsp.B); break;
                  case 1: WriteStr = UFunc.Comp(osp.P, nsp.P); break;
                  case 2: WriteStr = UFunc.Comp(osp.A, nsp.A); break;
                  case 3: WriteStr = UFunc.Comp(osp.J, nsp.J); break;
                  case 4: WriteStr = UFunc.Comp(osp.C, nsp.C); break;
                }
                (chr, num) = ("C", i % HandDBias);
              }


              if (WriteStr != string.Empty)
              {
                Parallel.For(0, svlist.Count, (s) =>
                {
                  if (SDAutoList.Contains(new KeyValuePair<string, int>(svlist[s].Name, i)))
                    svlist[s].Print("TRIS" + num.ToString() + "X" + WriteStr);
                });
              }
            }
          });
        }
      }
    }

  }

  internal class ASList
  {
    private List<string> SL;
    private List<int> IL;
    public int Count { get => SL.Count; }
    public List<string> Keys { get => SL; }
    public List<int> Values { get => IL; }
    public ASList()
    {
      SL = new List<string>();
      IL = new List<int>();
    }

    public bool Contains(KeyValuePair<string, int> k) => Contains(k.Key, k.Value);
    public bool Contains(string key, int value)
    {
      bool result = false;
      Parallel.For(0, SL.Count, (i) =>
      {
        if (SL[i] == key && IL[i] == value) result = true;
      });
      return result;
    }

    public void Remove(KeyValuePair<string, int> k) => Remove(k.Key, k.Value);
    public void Remove(string key, int? value = null)
    {
      if (SL == null || SL.Count <= 0) return;
      for (int i = SL.Count - 1; i >= 0; i--)
      {
        if (SL[i] == key && (value == null || IL[i] == value))
        {
          SL.RemoveAt(i);
          IL.RemoveAt(i);
        }
      }
    }

    public void Add(string key, int value)
    {
      SL.Add(key);
      IL.Add(value);
    }
  }
}
