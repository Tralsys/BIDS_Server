namespace TR.BIDSsv
{
  public static class ConstVals
  {
    /// <summary>BIDSで使用するコマンドの最小サイズ</summary>
    public static readonly int CMD_LEN_MIN = 4;//バージョンアップによって変わる可能性があるため, constは使用しない
    /// <summary>BIDSで使用するコマンドの最大サイズ</summary>
    public static readonly int CMD_LEN_MAX = 1024;//バージョンアップによって変わる可能性があるため, constは使用しない
    /// <summary>BIDSで使用する文字列コマンドの最大サイズ</summary>
    public static readonly int StringBuilder_Capacity = 512;
    public const string ToStrFormatInt = "D";
    public const string ToStrFormatFloat = "0.0#####";

    public const char NULL_CHAR = (char)0;

    public const char BIN_CMD_HEADER_0 = 't';
    public const char BIN_CMD_HEADER_1 = 'r';
    public const char STR_CMD_HEADER_0 = 'T';
    public const char STR_CMD_HEADER_1 = 'R';
    public const byte BIN_CMD_INFO_DATA = 0x62;
    public const byte BIN_CMD_PANEL_DATA = 0x70;
    public const byte BIN_CMD_SOUND_DATA = 0x73;

    public enum BIN_CMD_INFOD_TYPES
    {
      SPEC,
      STATE,
      BVE5D,
      OPEND,
      HANDLE
    }

    public const string CMD_HEADER = "TR";
    public const string GIPI_HEADER = "TO";
    public const string CMD_HEADER_ERROR = "TRE";

    public const string CRLF = "\r\n";

    /// <summary>Line Feed (\r)</summary>
    public const string CR_S = "\r";
    /// <summary>Carriage Return (\n)</summary>
    public const string LF_S = "\n";

    /// <summary>Line Feed (\r)</summary>
    public const char CR_C = '\r';
    /// <summary>Carriage Return (\n)</summary>
    public const char LF_C = '\n';

		#region CMD Type Chars
		public const char CMD_VERSION = 'V';
    public const char CMD_INFOREQ = 'I';
    public const char CMD_AUTOSEND_ADD = 'A';
    public const char CMD_AUTOSEND_DEL = 'D';
    public const char CMD_ERROR = 'E';
    public const char CMD_REVERSER = 'R';
    public const char CMD_POWER = 'P';
    public const char CMD_BREAK = 'B';
    public const char CMD_SPoleMC = 'S';
    public const char CMD_KeyCtrl = 'K';
		#endregion
		public const char CMD_SEPARATOR = 'X';

    public const int DNUM_POS = 4;

    #region DTYPE Chars
    public const int DTYPE_CHAR_POS = 3;//TRIx
    
    public const char DTYPE_ELAPD = 'E';
    public const char DTYPE_DOOR = 'D';
    public const char DTYPE_HANDPOS = 'H';
    public const char DTYPE_CONSTD = 'C';
    public const char DTYPE_PANEL = 'P';
    public const char DTYPE_SOUND = 'S';

    public const char DTYPE_PANEL_ARR = 'p';
    public const char DTYPE_SOUND_ARR = 's';
		#endregion

		/// <summary>Panelの連続出力機能で出力する数</summary>
		public const int PANEL_ARR_PRINT_COUNT = 32;
    /// <summary>Soundの連続出力機能で出力する数</summary>
		public const int SOUND_ARR_PRINT_COUNT = PANEL_ARR_PRINT_COUNT;

    /// <summary>Panel状態のBinary出力機能で出力する数</summary>
    public const int PANEL_BIN_ARR_PRINT_COUNT = 128;
    /// <summary>Sound状態のBinary出力機能で出力する数</summary>
    public const int SOUND_BIN_ARR_PRINT_COUNT = PANEL_BIN_ARR_PRINT_COUNT;

    
    public class DNums
    {
      public enum ElapD
      {
        Time_HMSms = -3,
        Pressures = -2,
        AllData = -1,
        Distance = 0,
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
        AllData = -1,
        Brake = 0,
        Power,
        Reverser,
        ConstSpd,
        SelfB
      }
      public enum ConstD
      {
        AllData = -1,
        Brake_Count = 0,
        Power_Count,
        ATSCheckPos,
        B67_Pos,
        Car_Count
      }
    }

    public static class BINARY_DATA_POS
    {
      public const int HEADER_0 = 0;
      public const int HEADER_1 = 1;

      public const int TYP = 2;
    }

    public static class BINARY_DATA
    {
      public const byte HEADER_0 = (byte)ASCII.t;
      public const byte HEADER_1 = (byte)ASCII.r;

      public const byte TYP_INFO_DATA = (byte)ASCII.b;
      public const byte TYP_PANEL_DATA = (byte)ASCII.p;
      public const byte TYP_SOUND_DATA = (byte)ASCII.s;
      public const byte TYP_REQUEST = (byte)ASCII.r;
      public const byte TYP_HANDLE_CTRL = (byte)ASCII.h;
      public const byte TYP_SECSYS_DATA = (byte)ASCII.a;
    }

    public enum ASCII : byte
    {
      D0=0x30,
      D1,
      D2,
      D3,
      D4,
      D5,
      D6,
      D7,
      D8,
      D9,

      A=0x41,
      B,
      C,
      D,
      E,
      F,
      G,
      H,
      I,
      J,
      K,
      L,
      M,
      N,
      O,
      P,
      Q,
      R,
      S,
      T,
      U,
      V,
      W,
      X,
      Y,
      Z,

      a =0x61,
      b,
      c,
      d,
      e,
      f,
      g,
      h,
      i,
      j,
      k,
      l,
      m,
      n,
      o,
      p,
      q,
      r,
      s,
      t,
      u,
      v,
      w,
      x,
      y,
      z
      
    }
  }
}
