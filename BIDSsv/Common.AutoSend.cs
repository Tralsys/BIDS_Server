using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
  //AutoSendに関連する実装

  static public partial class Common
  {
    /// <summary>BSMD変化時に発火</summary>
    static public event EventHandler<SMemLib.BSMDChangedEArgs> BSMDChanged
    {
      add => SMemLib.BIDSSMemChanged += value;
      remove => SMemLib.BIDSSMemChanged -= value;
    }
    /// <summary>openD変化時に発火</summary>
    static public event EventHandler<SMemLib.OpenDChangedEArgs> OpenDChanged
    {
      add => SMemLib.OpenDChanged += value;
      remove => SMemLib.OpenDChanged -= value;
    }
    /// <summary>PanelD変化時に発火</summary>
    static public event EventHandler<SMemLib.ArrayDChangedEArgs> PanelDChanged
    {
      add => SMemLib.PanelDChanged += value;
      remove => SMemLib.PanelDChanged -= value;
    }
    /// <summary>SoundD変化時に発火</summary>
    static public event EventHandler<SMemLib.ArrayDChangedEArgs> SoundDChanged
    {
      add => SMemLib.SoundDChanged += value;
      remove => SMemLib.SoundDChanged -= value;
    }

    /// <summary>AutoSendに関する各種定数や設定など</summary>
    public class AutoSendSetting
    {
      /// <summary>BasicConstデータ群のヘッダ</summary>
      public const uint HeaderBasicConst = 0x74726201;
      /// <summary>BasicConstデータ群のデータコマンド長</summary>
      public const uint BasicConstSize = 20;
      /// <summary>BasicCommonデータ群のヘッダ</summary>
      public const uint HeaderBasicCommon = 0x74726202;
      /// <summary>BasicCommonデータ群のデータコマンド長</summary>
      public const uint BasicCommonSize = 53;
      /// <summary>BasicBVE5</summary>
      public const uint HeaderBasicBVE5 = 0x74726203;
      /// <summary>BasicOBVE</summary>
      public const uint HeaderBasicOBVE = 0x74726204;
      /// <summary>BasicHandle</summary>
      public const uint HeaderBasicHandle = 0x74726205;
      /// <summary>BasicHandle</summary>
      public const uint BasicHandleSize = 20;
      /// <summary>PanelArr</summary>
      public const uint HeaderPanel = 0x74727000;
      /// <summary>PanelArr</summary>
      public const uint PanelSize = (ConstVals.PANEL_BIN_ARR_PRINT_COUNT + 1) * sizeof(int);
      /// <summary>SoundArr</summary>
      public const uint HeaderSound = 0x74727300;
      /// <summary>SoundArr</summary>
      public const uint SoundSize = (ConstVals.SOUND_BIN_ARR_PRINT_COUNT + 1) * sizeof(int);
      
      //デフォルトはtrue BIDS_Serverの場合はautodelで無効化する.
      /// <summary>BasicConstデータ群のAutoSend死活設定</summary>
      public static bool BasicConstAS = true;
      /// <summary>BasicCommonデータ群のAutoSend死活設定</summary>
      public static bool BasicCommonAS = true;
      /// <summary>BasicBVE5データ群のAutoSend死活設定</summary>
      public static bool BasicBVE5AS = true;
      /// <summary>BasicOBVEデータ群のAutoSend死活設定</summary>
      public static bool BasicOBVEAS = true;
      /// <summary>BasicHandleデータ群のAutoSend死活設定</summary>
      public static bool BasicHandleAS = true;
      /// <summary>BasicPanelArrデータ群のAutoSend死活設定</summary>
      public static bool BasicPanelAS = true;
      /// <summary>BasicSoundArrデータ群のAutoSend死活設定</summary>
      public static bool BasicSoundAS = true;

      /// <summary>BasicConstデータ群のデータコマンドを構成します.</summary>
      /// <param name="s">Specデータ</param>
      /// <param name="o">OpenDデータ</param>
      /// <returns>BasicConstデータ群のデータコマンド</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
      static public byte[] BasicConst(in Spec s, in OpenD o)
      {
        if (!BasicConstAS) return null;
        byte[] ba = new byte[BasicConstSize];
        int i = 0;
        ba.SetBIDSHeader(ConstVals.BIN_CMD_INFO_DATA, (byte)ConstVals.BIN_CMD_INFOD_TYPES.SPEC, ref i);
        ba.ValueSet2Arr((short)Version, ref i);
        ba.ValueSet2Arr((short)s.B, ref i);
        ba.ValueSet2Arr((short)s.P, ref i);
        ba.ValueSet2Arr((short)s.A, ref i);
        ba.ValueSet2Arr((short)s.J, ref i);
        ba.ValueSet2Arr((short)s.C, ref i);
        ba.ValueSet2Arr((short)o.SelfBCount, ref i);
        return ba;
      }
      /// <summary>BasicCommon</summary>
      /// <param name="s"></param>
      /// <param name="DoorState">ドアが閉扉状態か</param>
      /// <param name="SigNum">信号の現示番号</param>
      /// <returns>データコマンド</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
      static public byte[] BasicCommon(in State s, byte DoorState, int SigNum = 0)
      {
        if (!BasicCommonAS) return null;
        byte[] ba = new byte[BasicCommonSize];
        int i = 0;
        ba.SetBIDSHeader(ConstVals.BIN_CMD_INFO_DATA, (byte)ConstVals.BIN_CMD_INFOD_TYPES.STATE, ref i);
        ba.ValueSet2Arr((double)s.Z, ref i);
        ba.ValueSet2Arr((float)s.V, ref i);
        ba.ValueSet2Arr((float)s.I, ref i);
        ba.ValueSet2Arr((float)0.0, ref i);
        ba.ValueSet2Arr((int)s.T, ref i);
        ba.ValueSet2Arr((float)s.BC, ref i);
        ba.ValueSet2Arr((float)s.MR, ref i);
        ba.ValueSet2Arr((float)s.ER, ref i);
        ba.ValueSet2Arr((float)s.BP, ref i);
        ba.ValueSet2Arr((float)s.SAP, ref i);
        ba.ValueSet2Arr((int)SigNum, ref i);
        ba[i] = DoorState;
        return ba;
      }
      /// <summary>BVE5 Data Group</summary>
      /// <param name="o"></param>
      /// <returns>Data Command</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
      static public byte[] BasicBVE5(object o)
      {
        if (!BasicBVE5AS) return null;
        return null;
      }
      /// <summary>openBVE</summary>
      /// <param name="o"></param>
      /// <returns>Data Command</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
      static public byte[] BasicOBVE(OpenD o)
      {
        if (!BasicOBVEAS) return null;
        byte[] ba = new byte[Marshal.SizeOf(new OpenD()) + 4];
        int i = 0;
        ba.SetBIDSHeader(ConstVals.BIN_CMD_INFO_DATA, (byte)ConstVals.BIN_CMD_INFOD_TYPES.OPEND, ref i);
        
        IntPtr ip = new IntPtr();
        Marshal.StructureToPtr(o, ip, true);
        Marshal.Copy(ip, ba, i, ba.Length);
        return ba;
      }
      /// <summary>Handle State</summary>
      /// <param name="h"></param>
      /// <param name="SelfB"></param>
      /// <returns>Data Command</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
      static public byte[] BasicHandle(Hand h, int SelfB)
      {
        if (!BasicHandleAS) return null;
        byte[] ba = new byte[BasicHandleSize];
        int i = 0;
        ba.SetBIDSHeader(ConstVals.BIN_CMD_INFO_DATA, (byte)ConstVals.BIN_CMD_INFOD_TYPES.HANDLE, ref i);
        ba.ValueSet2Arr((int)h.P, ref i);
        ba.ValueSet2Arr((int)h.B, ref i);
        ba.ValueSet2Arr((int)h.R, ref i);
        ba.ValueSet2Arr((int)SelfB, ref i);
        return ba;
      }
      /// <summary>パネル</summary>
      /// <param name="a">Source Array</param>
      /// <param name="SttInd">Start Index Number</param>
      /// <returns>Result Array</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
      static public byte[] BasicPanel(int[] a, int SttInd)
      {
        if (!BasicPanelAS) return null;
        byte[] ba = new byte[PanelSize];
        Buffer.BlockCopy(HeaderPanel.GetBytes(), 0, ba, 0, sizeof(uint));
        ba[3] = (byte)SttInd;

        Parallel.For(0, ConstVals.PANEL_BIN_ARR_PRINT_COUNT, (i) =>
          {
            if (a.Length <= (SttInd * ConstVals.PANEL_BIN_ARR_PRINT_COUNT) + i) return;
            Buffer.BlockCopy(a[i].GetBytes(), 0, ba, (i + 1) * sizeof(int), sizeof(int));
          });

        return ba;
      }
      /// <summary>Sound Arr Auto Sender Command</summary>
      /// <param name="a"></param>
      /// <param name="SttInd"></param>
      /// <returns>Data Command</returns>
      [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
      static public byte[] BasicSound(int[] a, int SttInd)
      {
        if (!BasicSoundAS) return null;
        byte[] ba = new byte[SoundSize];

        _ = Parallel.For(0, ConstVals.SOUND_BIN_ARR_PRINT_COUNT, (i) =>
          {
            if (a.Length <= (SttInd * ConstVals.SOUND_BIN_ARR_PRINT_COUNT) + i) return;
            Buffer.BlockCopy(a[i].GetBytes(), 0, ba, (i + 1) * sizeof(int), sizeof(int));
          });

        Buffer.BlockCopy(HeaderSound.GetBytes(), 0, ba, 0, sizeof(uint));
        ba[3] = (byte)SttInd;
        return ba;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    static private void ASPtr(byte[] ba)
    {
      if (ba != null && ba.Length > 0 && svlist?.Count > 0)
        _ = Parallel.For(0, svlist.Count, (i) => svlist[i]?.Print(ba));
    }

    static private ASList PDAutoList = new ASList();
    static private ASList SDAutoList = new ASList();
    static private ASList AutoNumL = new ASList();

    /// <summary>配列を単位長さあたりで分割し, そこに変化があった場合に関数を実行する.</summary>
    /// <param name="OldArray"></param>
    /// <param name="NewArray"></param>
    /// <param name="OneTimePrintCount"></param>
    /// <param name="onChanged">単位配列長で分割し変化が検知されたとき, 何番目の配列で変化が検知されたかを示す引数とともに実行される関数</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    static void ArrDChangedCheck(in int[] OldArray, in int[] NewArray, int OneTimePrintCount, Action<int[], int> onChanged)
    {
      int al = Math.Max(OldArray.Length, NewArray.Length);
      if (al % OneTimePrintCount != 0) al = ((int)Math.Floor((double)al / OneTimePrintCount) + 1) * OneTimePrintCount;

      int[] oa = new int[al];
      int[] na = new int[al];
      Buffer.BlockCopy(OldArray, 0, oa, 0, OldArray.Length);
      Buffer.BlockCopy(NewArray, 0, na, 0, NewArray.Length);
      _ = Parallel.For(0, al / OneTimePrintCount, (i) =>
        {
          int j = i * OneTimePrintCount;
          if (!UFunc.ArrayEqual(oa, j, na, j, OneTimePrintCount)) onChanged.Invoke(na, i);
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    private static void Common_SoundDChanged(object sender, SMemLib.ArrayDChangedEArgs e)
    {
      if (!IsStarted || !(svlist?.Count > 0)) return;
      _ = Task.Run(() => Parallel.Invoke(
          () => Parallel.For(0, svlist.Count, (i) => svlist[i].OnSoundDChanged(in e.NewArray)),
          () =>
          {
            if (AutoSendSetting.BasicSoundAS)
              ArrDChangedCheck(e.OldArray, e.NewArray, ConstVals.SOUND_BIN_ARR_PRINT_COUNT, (na, i) => ASPtr(AutoSendSetting.BasicSound(na, i)));
          },
          () =>
          {
            if (!(SDAutoList?.Count > 0)) return;
            Parallel.For(0, Math.Max(e.OldArray.Length, e.NewArray.Length), (i) =>
            {
              int? Num = null;
              if (e.OldArray.Length <= i) Num = e.NewArray[i];
              else if (e.NewArray.Length > i && e.OldArray[i] != e.NewArray[i]) Num = e.NewArray[i];

              if (Num != null) SDAutoList.PrintValue(UFunc.BIDSCMDMaker(ConstVals.CMD_INFOREQ, ConstVals.DTYPE_SOUND, i, Num.ToString()), i, ConstVals.DTYPE_SOUND);
            });
          },
          () =>
          {
            if (!(SDAutoList?.Count > 0)) return;
            ArrDChangedCheck(e.OldArray, e.NewArray, ConstVals.SOUND_ARR_PRINT_COUNT,
              (_, i) =>
              {
                string s = null;
                if (Get_TRI_Data(out s, ConstVals.DTYPE_SOUND_ARR, i, true)) SDAutoList.PrintValue(UFunc.BIDSCMDMaker(ConstVals.CMD_INFOREQ, ConstVals.DTYPE_SOUND_ARR, i, s), i, ConstVals.DTYPE_SOUND_ARR);
              });
          }
          ));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    private static void Common_PanelDChanged(object sender, SMemLib.ArrayDChangedEArgs e)
    {
      if (!IsStarted || (svlist?.Count > 0)) return;
      _ = Task.Run(() => Parallel.Invoke(
          () => _ = Parallel.For(0, svlist.Count, (i) => svlist[i].OnPanelDChanged(in e.NewArray)),
          () =>
          {
            if (AutoSendSetting.BasicPanelAS)
              ArrDChangedCheck(e.OldArray, e.NewArray, ConstVals.PANEL_BIN_ARR_PRINT_COUNT, (na, i) => ASPtr(AutoSendSetting.BasicPanel(na, i)));
          },
          () =>
          {
            if (!(PDAutoList?.Count > 0)) return;
            _ = Parallel.For(0, Math.Max(e.OldArray.Length, e.NewArray.Length), (i) =>
              {
                int? Num = null;
                if (e.OldArray.Length <= i) Num = e.NewArray[i];
                else if (e.NewArray.Length > i && e.OldArray[i] != e.NewArray[i]) Num = e.NewArray[i];

                if (Num != null) PDAutoList.PrintValue(UFunc.BIDSCMDMaker(ConstVals.CMD_INFOREQ, ConstVals.DTYPE_PANEL, i, Num.ToString()), i, ConstVals.DTYPE_PANEL);
              });
          },
          () =>
          {
            if (!(PDAutoList?.Count > 0)) return;
            ArrDChangedCheck(e.OldArray, e.NewArray, ConstVals.PANEL_ARR_PRINT_COUNT,
              (_, i) =>
              {
                string s = null;
                if (Get_TRI_Data(out s, ConstVals.DTYPE_PANEL_ARR, i, true)) SDAutoList.PrintValue(UFunc.BIDSCMDMaker(ConstVals.CMD_INFOREQ, ConstVals.DTYPE_PANEL_ARR, i, s), i, ConstVals.DTYPE_PANEL_ARR);
              });
          }));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    private static void Common_OpenDChanged(object sender, SMemLib.OpenDChangedEArgs e)
    {
      if (!IsStarted || !(svlist?.Count>0)) return;
      _ = Task.Run(() => Parallel.Invoke(
        () =>
        {
          if (AutoSendSetting.BasicOBVEAS && !Equals(e.OldData.SelfBPosition, e.NewData.SelfBPosition))
            ASPtr(AutoSendSetting.BasicHandle(BSMD.HandleData, e.NewData.SelfBPosition));
        },
        () => Parallel.For(0, svlist.Count, (i) => svlist[i].OnOpenDChanged(in e.NewData))
        ));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    private static void Common_BSMDChanged(object sender, SMemLib.BSMDChangedEArgs e)
    {
      if (!IsStarted || !(svlist?.Count > 0)) return;
      _ = Task.Run(() => Parallel.Invoke(
          () => Parallel.For(0, svlist.Count, (i) => svlist[i].OnBSMDChanged(in e.NewData)),
          () =>
          {
            if (AutoSendSetting.BasicConstAS && !Equals(e.OldData.SpecData, e.NewData.SpecData))
              ASPtr(AutoSendSetting.BasicConst(e.NewData.SpecData, OD));
          },
          () =>
          {
            if (AutoSendSetting.BasicCommonAS && (!Equals(e.OldData.StateData, e.NewData.StateData) || e.OldData.IsDoorClosed != e.NewData.IsDoorClosed))
              ASPtr(AutoSendSetting.BasicCommon(e.NewData.StateData, e.NewData.IsDoorClosed ? ConstVals.TRUE_VALUE : ConstVals.FALSE_VALUE));
          },
          () =>
          {
            if (AutoSendSetting.BasicHandleAS && !Equals(e.NewData.HandleData, e.OldData.HandleData))
              ASPtr(AutoSendSetting.BasicHandle(e.NewData.HandleData, OD.SelfBPosition));
          },
          () =>
          {
            if (!(AutoNumL?.Count > 0)) return;
            _=Parallel.For(0, AutoNumL.Count, (ind) =>
            {
              ASList.Elem elem = AutoNumL.ElementAt(ind);

              if (elem.sv?.IsDisposed != false) return;//null=>return, Disposed=T:return, Disposed=F:next

            string data = string.Empty;
              try
              {
                if (!Get_TRI_Data(out data, elem.DType, elem.DNum)) return;
              }
              catch (Exception e) { Console.WriteLine("Common.AutoSend.Common_BSMDChanged : {0}", e); }

              if (string.IsNullOrWhiteSpace(data)) return;

              TimeSpan ots = TimeSpan.FromMilliseconds(e.OldData.StateData.T);
              TimeSpan nts = TimeSpan.FromMilliseconds(e.NewData.StateData.T);

              if (elem.DType switch
              {
                ConstVals.DTYPE_CONSTD => (ConstVals.DNums.ConstD)elem.DNum switch
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

                ConstVals.DTYPE_ELAPD => (ConstVals.DNums.ElapD)elem.DNum switch
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

                ConstVals.DTYPE_HANDPOS => (ConstVals.DNums.HandPos)elem.DNum switch
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
                == false) elem.sv.Print(UFunc.BIDSCMDMaker(ConstVals.CMD_INFOREQ, elem.DType, elem.DNum, data));
            });
          }
        ));
    }

  }

}
