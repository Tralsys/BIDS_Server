using System;
using System.Collections.Generic;
using System.Text;

namespace TR.BIDSsv
{
  static public partial class Common
  {
    public const string ToStrFormatInt = "D";
    public const string ToStrFormatFloat = ".0#####";
    static public string DataPicker(char DType, int DNum, string ToStringFormatInt = ToStrFormatInt, string ToStringFormatFloat = ToStrFormatFloat)
    {
      if (string.IsNullOrWhiteSpace(ToStringFormatInt)) ToStringFormatInt = ToStrFormatInt;
      if (string.IsNullOrWhiteSpace(ToStringFormatFloat)) ToStringFormatFloat = ToStrFormatFloat;

      bool IsFloatVal = false;
      object o = DataPicker(DType, DNum, out IsFloatVal);
      if (o == null) return null;
      if (IsFloatVal)
      {
        Type otyp = o.GetType();
        if (otyp == typeof(float))
          return ((float)o).ToString(ToStringFormatFloat);
        if (otyp == typeof(double))
          return ((double)o).ToString(ToStringFormatFloat);
      }

      return ((int)o).ToString(ToStringFormatInt);
    }

    static public object DataPicker(char DType, int DNum, out bool IsFloatVal)
    {
      IsFloatVal = new bool();
      
      IsFloatVal = false;//init
      switch (DType)
      {
        case ConstVals.DTYPE_CONSTD:
          return BSMD.SpecData.PickData(DType, DNum, out IsFloatVal);

        case ConstVals.DTYPE_DOOR:
          return BSMD.IsDoorClosed ? 1 : 0;

        case ConstVals.DTYPE_ELAPD:
          return BSMD.StateData.PickData(DType, DNum, out IsFloatVal);

        case ConstVals.DTYPE_HANDPOS:
          return BSMD.HandleData.PickData(DType, DNum, out IsFloatVal);

        case ConstVals.DTYPE_PANEL:
          return DNum >= PD.Length ? 0 : PD.Panels[DNum];

        case ConstVals.DTYPE_SOUND:
          return DNum >= SD.Length ? 0 : SD.Sounds[DNum];
      }
      return null;
    }

    /// <summary>
    /// Returns the data that matches the data specification information.
    /// </summary>
    /// <param name="inData">Data Source</param>
    /// <param name="DType">Data Type specification char</param>
    /// <param name="DNum">Data Number</param>
    /// <param name="IsFloatVal">set value that whether the return value is float</param>
    /// <returns>result value or null</returns>
    public static object PickData(this in Spec inData, char DType, int DNum, out bool IsFloatVal)
    {
      IsFloatVal = new bool();
      if (DType != ConstVals.DTYPE_CONSTD) return null;

      IsFloatVal = false;//SpecData has no floating point number

      switch ((ConstVals.DNums.ConstD)DNum)
      {
        case ConstVals.DNums.ConstD.ATSCheckPos:
          return inData.A;
        case ConstVals.DNums.ConstD.B67_Pos:
          return inData.J;
        case ConstVals.DNums.ConstD.Brake_Count:
          return inData.B;
        case ConstVals.DNums.ConstD.Car_Count:
          return inData.C;
        case ConstVals.DNums.ConstD.Power_Count:
          return inData.P;
      }

      return null;//default
    }
    /// <summary>
    /// Returns the data that matches the data specification information.
    /// </summary>
    /// <param name="inData">Data Source</param>
    /// <param name="DType">Data Type specification char</param>
    /// <param name="DNum">Data Number</param>
    /// <param name="IsFloatVal">set value that whether the return value is float</param>
    /// <returns>result value or null</returns>
    public static object PickData(this in State inData, char DType, int DNum, out bool IsFloatVal)
    {
      IsFloatVal = new bool();
      if (DType != ConstVals.DTYPE_ELAPD) return null;

      IsFloatVal = false;//initialize

      switch ((ConstVals.DNums.ElapD)DNum)
      {
        case ConstVals.DNums.ElapD.BC_Pres:
          IsFloatVal = true;
          return inData.BC;
        case ConstVals.DNums.ElapD.BP_Pres:
          IsFloatVal = true;
          return inData.BP;
        case ConstVals.DNums.ElapD.Current:
          IsFloatVal = true;
          return inData.I;
        case ConstVals.DNums.ElapD.Distance:
          IsFloatVal = true;
          return inData.Z;
        case ConstVals.DNums.ElapD.ER_Pres:
          IsFloatVal = true;
          return inData.ER;
        case ConstVals.DNums.ElapD.MR_Pres:
          IsFloatVal = true;
          return inData.MR;
        case ConstVals.DNums.ElapD.SAP_Pres:
          IsFloatVal = true;
          return inData.SAP;
        case ConstVals.DNums.ElapD.Speed:
          IsFloatVal = true;
          return inData.V;
        case ConstVals.DNums.ElapD.Time:
          IsFloatVal = false;
          return inData.T;
        case ConstVals.DNums.ElapD.Voltage:
          IsFloatVal = true;
          return 0.0f;//not implemented
      }

      var t = TimeSpan.FromMilliseconds(inData.T);
      IsFloatVal = false;//time data is always INT value
      switch ((ConstVals.DNums.ElapD)DNum)
      {
        case ConstVals.DNums.ElapD.TIME_Hour:
          return t.Hours;
        case ConstVals.DNums.ElapD.TIME_Min:
          return t.Minutes;
        case ConstVals.DNums.ElapD.TIME_MSec:
          return t.Milliseconds;
        case ConstVals.DNums.ElapD.TIME_Sec:
          return t.Seconds;
      }

      return null;//default
    }
    /// <summary>
    /// Returns the data that matches the data specification information.
    /// </summary>
    /// <param name="inData">Data Source</param>
    /// <param name="DType">Data Type specification char</param>
    /// <param name="DNum">Data Number</param>
    /// <param name="IsFloatVal">set value that whether the return value is float</param>
    /// <returns>result value or null</returns>
    public static object PickData(this in OpenD inData, char DType, int DNum, out bool IsFloatVal)
    {
      IsFloatVal = new bool();
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
    public static object PickData(this in Hand inData, char DType, int DNum, out bool IsFloatVal)
    {
      IsFloatVal = new bool();
      if (DType != ConstVals.DTYPE_HANDPOS) return null;

      IsFloatVal = false;//HandPos is always INT

      switch ((ConstVals.DNums.HandPos)DNum)
      {
        case ConstVals.DNums.HandPos.Brake:
          return inData.B;
        case ConstVals.DNums.HandPos.ConstSpd:
          return inData.C;
        case ConstVals.DNums.HandPos.Power:
          return inData.P;
        case ConstVals.DNums.HandPos.Reverser:
          return inData.R;
      }

      return null;//default
    }

  }
}
