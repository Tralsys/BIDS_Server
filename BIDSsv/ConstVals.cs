using System;
using System.Collections.Generic;
using System.Text;

namespace TR.BIDSsv
{
  public static class ConstVals
  {
    public const string CMD_HEADER = "TR";

    public const char CMD_VERSION = 'V';
    public const char CMD_INFOREQ = 'I';
    public const char CMD_AUTOSEND_ADD = 'A';
    public const char CMD_AUTOSEND_DEL = 'D';
    public const char CMD_ERROR = 'E';

    public const char CMD_SEPARATOR = 'X';

    public const char DTYPE_ELAPD = 'E';
    public const char DTYPE_DOOR = 'D';
    public const char DTYPE_HANDPOS = 'H';
    public const char DTYPE_CONSTD = 'C';
    public const char DTYPE_PANEL = 'P';
    public const char DTYPE_SOUND = 'S';

    public class DNums
    {
      public enum ElapD
      {
        Distance,
        Speed,
        Time,
        BC_Pres,
        MR_Pres,
        ER_Pres,
        BP_Pres,
        SAP_Pres,
        Current,
        Voltage,
        TIME_Hour,
        TIME_Min,
        TIME_Sec,
        TIME_MSec
      }
      public enum HandPos
      {
        Brake,
        Power,
        Reverser,
        ConstSpd
      }
      public enum ConstD
      {
        Brake_Count,
        Power_Count,
        ATSCheckPos,
        B67_Pos,
        Car_Count
      }
    }
  }
}
