namespace TR.BIDSsv
{
	/// <summary>BIDSsvで使用する定数値が含まれます.</summary>
	public static class ConstVals
	{
		public static readonly int DefPNum = 14147;

		/// <summary>BIDSで使用するコマンドの最小サイズ</summary>
		public static readonly int CMD_LEN_MIN = 4;//バージョンアップによって変わる可能性があるため, constは使用しない
		/// <summary>BIDSで使用するコマンドの最大サイズ</summary>
		public static readonly int CMD_LEN_MAX = 1024;//バージョンアップによって変わる可能性があるため, constは使用しない
		/// <summary>BIDSで使用する文字列コマンドの最大サイズ</summary>
		public static readonly int StringBuilder_Capacity = 512;
		/// <summary>NULL文字</summary>
		public const char NULL_CHAR = (char)0;
		/// <summary>TRUEを意味する値</summary>
		public const byte TRUE_VALUE = 1;
		/// <summary>FALSEを意味する値</summary>
		public const byte FALSE_VALUE = 0;
		/// <summary>Binaryデータコマンドのヘッダ0</summary>
		public static readonly char BIN_CMD_HEADER_0 = 't';
		/// <summary>Binary Data CommandのHeader1</summary>
		public static readonly char BIN_CMD_HEADER_1 = 'R';//20200524: ビッグエンディアン採用につきBINコマンドのヘッダ変更
		/// <summary>String Data CommandのHeader0</summary>
		public static readonly char STR_CMD_HEADER_0 = 'T';
		/// <summary>String Data CommandのHeader1</summary>
		public static readonly char STR_CMD_HEADER_1 = 'R';
		/// <summary>BDCのデータ識別子(INFO Data)</summary>
		public static readonly byte BIN_CMD_INFO_DATA = 0x62;
		/// <summary>BDCのデータ識別子(Panel Data)</summary>
		public static readonly byte BIN_CMD_PANEL_DATA = 0x70;
		/// <summary>BDCのデータ識別子(Sound Data)</summary>
		public static readonly byte BIN_CMD_SOUND_DATA = 0x73;

		/// <summary>BDCのInfoDの種類一覧</summary>
		public enum BIN_CMD_INFOD_TYPES
		{
			/// <summary>Specデータ</summary>
			SPEC,
			/// <summary>Stateデータ</summary>
			STATE,
			/// <summary>BVE5独自データ</summary>
			BVE5D,
			/// <summary>openBVE独自データ</summary>
			OPEND,
			/// <summary>ハンドル位置データ</summary>
			HANDLE
		}

		/// <summary>BIDS String CommandのHeader</summary>
		public const string CMD_HEADER = "TR";
		/// <summary>GIPI CommandのHeader</summary>
		public const string GIPI_HEADER = "TO";
		/// <summary>BIDS String CommandのError Command Header</summary>
		public const string CMD_HEADER_ERROR = "TRE";

		/// <summary>CRLF</summary>
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
		/// <summary>String Command - Version</summary>
		public const char CMD_VERSION = 'V';
		/// <summary>String Command - Info Request</summary>
		public const char CMD_INFOREQ = 'I';
		/// <summary>String Command - AutoSend Add Request</summary>
		public const char CMD_AUTOSEND_ADD = 'A';
		/// <summary>String Command - AutoSend Remove Request</summary>
		public const char CMD_AUTOSEND_DEL = 'D';
		/// <summary>String Command - Error report</summary>
		public const char CMD_ERROR = 'E';
		/// <summary>String Command - Reverser Control Command</summary>
		public const char CMD_REVERSER = 'R';
		/// <summary>String Command - Power Lever Control Command</summary>
		public const char CMD_POWER = 'P';
		/// <summary>String Command - Brake Lever Control Command</summary>
		public const char CMD_BREAK = 'B';
		/// <summary>String Command - Single Pole MasterController Control Command</summary>
		public const char CMD_SPoleMC = 'S';
		/// <summary>String Command - Key Control Command</summary>
		public const char CMD_KeyCtrl = 'K';
		#endregion
		/// <summary>Separator between request command and response data</summary>
		public const char CMD_SEPARATOR = 'X';

		/// <summary>Position of DNum in the command</summary>
		public const int DNUM_POS = 4;

		#region DTYPE Chars
		/// <summary>Position of DType char in the command</summary>
		public const int DTYPE_CHAR_POS = 3;//TRIx
		/// <summary>DType Char - ElapD</summary>
		public const char DTYPE_ELAPD = 'E';
		/// <summary>DType Char - Door State</summary>
		public const char DTYPE_DOOR = 'D';
		/// <summary>DType Char - Handle Position Data</summary>
		public const char DTYPE_HANDPOS = 'H';
		/// <summary>DType Char - Const Data</summary>
		public const char DTYPE_CONSTD = 'C';
		/// <summary>DType Char - Panel Data</summary>
		public const char DTYPE_PANEL = 'P';
		/// <summary>DType Char - Sound Data</summary>
		public const char DTYPE_SOUND = 'S';
		/// <summary>DType Char - Panel Array</summary>
		public const char DTYPE_PANEL_ARR = 'p';
		/// <summary>DType Char - Sound Array</summary>
		public const char DTYPE_SOUND_ARR = 's';
		#endregion

		/// <summary>Panelの連続出力機能で出力する数</summary>
		public static readonly int PANEL_ARR_PRINT_COUNT = 32;
		/// <summary>Soundの連続出力機能で出力する数</summary>
		public static readonly int SOUND_ARR_PRINT_COUNT = PANEL_ARR_PRINT_COUNT;

		/// <summary>Panel状態のBinary出力機能で出力する数</summary>
		public static readonly int PANEL_BIN_ARR_PRINT_COUNT = 128;
		/// <summary>Sound状態のBinary出力機能で出力する数</summary>
		public static readonly int SOUND_BIN_ARR_PRINT_COUNT = PANEL_BIN_ARR_PRINT_COUNT;

		/// <summary>String InfoReqのDNumに入る数値</summary>
		public class DNums
		{
			/// <summary>ElapDに使用するDNum</summary>
			public enum ElapD
			{
				/// <summary>時刻情報(hh:mm:ss.fff)</summary>
				Time_HMSms = -3,
				/// <summary>圧力一覧 [kPa]</summary>
				Pressures = -2,
				/// <summary>ElapDの全データ</summary>
				AllData = -1,
				/// <summary>列車位置 [m]</summary>
				Distance = 0,
				/// <summary>列車速度 [km/h]</summary>
				Speed,
				/// <summary>0時からの経過時間 [ms]</summary>
				Time,
				/// <summary>BC Pressure [kPa]</summary>
				BC_Pres,
				/// <summary>MR Pressure [kPa]</summary>
				MR_Pres,
				/// <summary>ER Pressure [kPa]</summary>
				ER_Pres,
				/// <summary>BP Pressure [kPa]</summary>
				BP_Pres,
				/// <summary>SAP Pressure [kPa]</summary>
				SAP_Pres,
				/// <summary>電流値 [A]</summary>
				Current,
				/// <summary>架線電圧 [V]</summary>
				Voltage,
				/// <summary>現在時刻 (hh)</summary>
				TIME_Hour,
				/// <summary>現在時刻 (mm)</summary>
				TIME_Min,
				/// <summary>現在時刻 (ss)</summary>
				TIME_Sec,
				/// <summary>現在時刻 (fff)</summary>
				TIME_MSec
			}
			/// <summary>ハンドル位置情報</summary>
			public enum HandPos
			{
				/// <summary>Handleデータ一覧</summary>
				AllData = -1,
				/// <summary>制動ハンドル位置</summary>
				Brake = 0,
				/// <summary>力行ハンドル位置</summary>
				Power,
				/// <summary>逆転ハンドル位置</summary>
				Reverser,
				/// <summary>定速制御状態</summary>
				ConstSpd,
				/// <summary>単弁ハンドル位置</summary>
				SelfB
			}
			/// <summary>車両固有情報</summary>
			public enum ConstD
			{
				/// <summary>全データ</summary>
				AllData = -1,
				/// <summary>制動段数</summary>
				Brake_Count = 0,
				/// <summary>力行段数</summary>
				Power_Count,
				/// <summary>ATS確認位置</summary>
				ATSCheckPos,
				/// <summary>制動67°位置</summary>
				B67_Pos,
				/// <summary>編成両数</summary>
				Car_Count
			}
		}

		/// <summary>Binary Data Commandの各データ位置情報</summary>
		public static class BINARY_DATA_POS
		{
			/// <summary>Position of Header0</summary>
			public const int HEADER_0 = 0;
			/// <summary>Position of Header1</summary>
			public const int HEADER_1 = 1;

			/// <summary>Position of Data Type</summary>
			public const int TYP = 2;
		}

		/// <summary>byte型での各char</summary>
		public static class BINARY_DATA
		{
			/// <summary>Header0 ('t'=116)</summary>
			public const byte HEADER_0 = (byte)ASCII.t;
			/// <summary>Header1 ('r'=114)</summary>
			public const byte HEADER_1 = (byte)ASCII.r;
			/// <summary>Info Data</summary>
			public const byte TYP_INFO_DATA = (byte)ASCII.b;
			/// <summary>Panel Data</summary>
			public const byte TYP_PANEL_DATA = (byte)ASCII.p;
			/// <summary>Sound Data</summary>
			public const byte TYP_SOUND_DATA = (byte)ASCII.s;
			/// <summary>Data Request</summary>
			public const byte TYP_REQUEST = (byte)ASCII.r;
			/// <summary>Handle Control Request</summary>
			public const byte TYP_HANDLE_CTRL = (byte)ASCII.h;
			/// <summary>Security System Data Request</summary>
			public const byte TYP_SECSYS_DATA = (byte)ASCII.a;
		}

		/// <summary>ASCIIコードを列挙</summary>
		public enum ASCII : byte
		{
			/// <summary>0</summary>
			D0 = (byte)'0',
			/// <summary>1</summary>
			D1,
			/// <summary>2</summary>
			D2,
			/// <summary>3</summary>
			D3,
			/// <summary>4</summary>
			D4,
			/// <summary>5</summary>
			D5,
			/// <summary>6</summary>
			D6,
			/// <summary>7</summary>
			D7,
			/// <summary>8</summary>
			D8,
			/// <summary>9</summary>
			D9,

			/// <summary>A</summary>
			A = (byte)'A',
			/// <summary>B</summary>
			B,
			/// <summary>C</summary>
			C,
			/// <summary>D</summary>
			D,
			/// <summary>E</summary>
			E,
			/// <summary>F</summary>
			F,
			/// <summary>G</summary>
			G,
			/// <summary>H</summary>
			H,
			/// <summary>I</summary>
			I,
			/// <summary>J</summary>
			J,
			/// <summary>K</summary>
			K,
			/// <summary>L</summary>
			L,
			/// <summary>M</summary>
			M,
			/// <summary>N</summary>
			N,
			/// <summary>O</summary>
			O,
			/// <summary>P</summary>
			P,
			/// <summary>Q</summary>
			Q,
			/// <summary>R</summary>
			R,
			/// <summary>S</summary>
			S,
			/// <summary>T</summary>
			T,
			/// <summary>U</summary>
			U,
			/// <summary>V</summary>
			V,
			/// <summary>W</summary>
			W,
			/// <summary>X</summary>
			X,
			/// <summary>Y</summary>
			Y,
			/// <summary>Z</summary>
			Z,

			/// <summary>a</summary>
			a = (byte)'a',
			/// <summary>b</summary>
			b,
			/// <summary>c</summary>
			c,
			/// <summary>d</summary>
			d,
			/// <summary>e</summary>
			e,
			/// <summary>f</summary>
			f,
			/// <summary>g</summary>
			g,
			/// <summary>h</summary>
			h,
			/// <summary>i</summary>
			i,
			/// <summary>j</summary>
			j,
			/// <summary>k</summary>
			k,
			/// <summary>l</summary>
			l,
			/// <summary>m</summary>
			m,
			/// <summary>n</summary>
			n,
			/// <summary>o</summary>
			o,
			/// <summary>p</summary>
			p,
			/// <summary>q</summary>
			q,
			/// <summary>r</summary>
			r,
			/// <summary>s</summary>
			s,
			/// <summary>t</summary>
			t,
			/// <summary>u</summary>
			u,
			/// <summary>v</summary>
			v,
			/// <summary>w</summary>
			w,
			/// <summary>x</summary>
			x,
			/// <summary>y</summary>
			y,
			/// <summary>z</summary>
			z
			
		}
	}
}
