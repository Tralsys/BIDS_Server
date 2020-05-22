using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSsv
{
  static public partial class Common
  {
    static public object DataPicker(in char DType, in int DNum)
    {
      switch (DType)
      {
        case ConstVals.DTYPE_CONSTD:
          return BSMD.SpecData.PickData(DType, DNum);

        case ConstVals.DTYPE_DOOR:
          return BSMD.IsDoorClosed ? 1 : 0;

        case ConstVals.DTYPE_ELAPD:
          return BSMD.StateData.PickData(DType, DNum);

        case ConstVals.DTYPE_HANDPOS:
          return BSMD.HandleData.PickData(DType, DNum);

        case ConstVals.DTYPE_PANEL:
          return DNum >= PD.Length ? 0 : PD.Panels[DNum];

        case ConstVals.DTYPE_SOUND:
          return DNum >= SD.Length ? 0 : SD.Sounds[DNum];

        case ConstVals.DTYPE_PANEL_ARR:
          if (DNum < 0) return ConstVals.PANEL_ARR_PRINT_COUNT;
          int[] retArrP = new int[ConstVals.PANEL_ARR_PRINT_COUNT];
          int startPosP = DNum * ConstVals.PANEL_ARR_PRINT_COUNT;
          if (startPosP >= PD.Length) return retArrP;//送信要素の始まりがSrc配列最後より後

          Buffer.BlockCopy(PD.Panels, startPosP, retArrP, 0,
            ((startPosP + ConstVals.PANEL_ARR_PRINT_COUNT) > PD.Length ?
            PD.Length - startPosP : ConstVals.PANEL_ARR_PRINT_COUNT) * sizeof(int));

          return retArrP;

        case ConstVals.DTYPE_SOUND_ARR:
          if (DNum < 0) return ConstVals.SOUND_ARR_PRINT_COUNT;
          int[] retArrS = new int[ConstVals.SOUND_ARR_PRINT_COUNT];
          int startPosS = DNum * ConstVals.SOUND_ARR_PRINT_COUNT;
          if (startPosS >= SD.Length) return retArrS;//送信要素の始まりがSrc配列最後より後

          Buffer.BlockCopy(SD.Sounds, startPosS, retArrS, 0,
            ((startPosS + ConstVals.SOUND_ARR_PRINT_COUNT) > SD.Length ?
            SD.Length - startPosS : ConstVals.SOUND_ARR_PRINT_COUNT) * sizeof(int));

          return retArrS;
      }
      return null;
    }

    /// <summary>
    /// Returns the data that matches the data specification information.
    /// </summary>
    /// <param name="inData">Data Source</param>
    /// <param name="DType">Data Type specification char</param>
    /// <param name="DNum">Data Number</param>
    /// <returns>result value or null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    public static object PickData(this in Spec inData, in char DType, in int DNum)
    {
      if (DType != ConstVals.DTYPE_CONSTD) return null;

      return (ConstVals.DNums.ConstD)DNum switch
      {
        ConstVals.DNums.ConstD.ATSCheckPos => inData.A,
        ConstVals.DNums.ConstD.B67_Pos => inData.J,
        ConstVals.DNums.ConstD.Brake_Count => inData.B,
        ConstVals.DNums.ConstD.Car_Count => inData.C,
        ConstVals.DNums.ConstD.Power_Count => inData.P,
        ConstVals.DNums.ConstD.AllData => inData,
        _ => null
      };
    }
    /// <summary>
    /// Returns the data that matches the data specification information.
    /// </summary>
    /// <param name="inData">Data Source</param>
    /// <param name="DType">Data Type specification char</param>
    /// <param name="DNum">Data Number</param>
    /// <param name="IsFloatVal">set value that whether the return value is float</param>
    /// <returns>result value or null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    public static object PickData(this in State inData, in char DType, in int DNum)
    {
      if (DType != ConstVals.DTYPE_ELAPD) return null;

      var t = TimeSpan.FromMilliseconds(inData.T);
      return (ConstVals.DNums.ElapD)DNum switch
      {
        ConstVals.DNums.ElapD.BC_Pres =>
          inData.BC,
        ConstVals.DNums.ElapD.BP_Pres =>
          inData.BP,
        ConstVals.DNums.ElapD.Current =>
          inData.I,
        ConstVals.DNums.ElapD.Distance =>
          inData.Z,
        ConstVals.DNums.ElapD.ER_Pres =>
          inData.ER,
        ConstVals.DNums.ElapD.MR_Pres =>
          inData.MR,
        ConstVals.DNums.ElapD.SAP_Pres =>
          inData.SAP,
        ConstVals.DNums.ElapD.Speed =>
          inData.V,
        ConstVals.DNums.ElapD.Time =>
          inData.T,
        ConstVals.DNums.ElapD.Voltage =>
          0.0f,//not implemented
        ConstVals.DNums.ElapD.TIME_Hour =>
          t.Hours,
        ConstVals.DNums.ElapD.TIME_Min =>
          t.Minutes,
        ConstVals.DNums.ElapD.TIME_MSec =>
          t.Milliseconds,
        ConstVals.DNums.ElapD.TIME_Sec =>
         t.Seconds,
        ConstVals.DNums.ElapD.AllData => inData,
        ConstVals.DNums.ElapD.Time_HMSms => t,
        ConstVals.DNums.ElapD.Pressures => new float[5] { inData.BC, inData.MR, inData.ER, inData.BP, inData.SAP },
        _ => null
      };
    }
    /// <summary>
    /// Returns the data that matches the data specification information.
    /// </summary>
    /// <param name="inData">Data Source</param>
    /// <param name="DType">Data Type specification char</param>
    /// <param name="DNum">Data Number</param>
    /// <param name="IsFloatVal">set value that whether the return value is float</param>
    /// <returns>result value or null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    public static object PickData(this in OpenD inData, in char DType, in int DNum)
    {
      return null;//Not implemented
    }
    /// <summary>
    /// Returns the data that matches the data specification information.
    /// </summary>
    /// <param name="inData">Data Source</param>
    /// <param name="DType">Data Type specification char</param>
    /// <param name="DNum">Data Number</param>
    /// <param name="IsFloatVal">set value that whether the return value is float</param>
    /// <returns>result value or null</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    public static object PickData(this in Hand inData, in char DType, in int DNum)
    {
      if (DType != ConstVals.DTYPE_HANDPOS) return null;

      return (ConstVals.DNums.HandPos)DNum switch
      {
        ConstVals.DNums.HandPos.Brake => inData.B,
        ConstVals.DNums.HandPos.ConstSpd => inData.C,
        ConstVals.DNums.HandPos.Power => inData.P,
        ConstVals.DNums.HandPos.Reverser => inData.R,
        ConstVals.DNums.HandPos.AllData => inData,
        _ => null
      };
    }

  }
}
