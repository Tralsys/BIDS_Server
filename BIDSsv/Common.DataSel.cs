using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
  static public partial class Common
  {
		#region String型を使用したMethod群

    /// <summary>ヘッダがTOであるコマンドでの要求を処理する.</summary>
    /// <param name="GotStr">接尾辞が取り除かれたコマンド文字列</param>
    /// <returns>返信すべき文字列(なければnull)</returns>
    static private string DataSelTO(in string GotStr)
    {
      if (string.IsNullOrWhiteSpace(GotStr)) return null;
      char[] GotChars = GotStr.ToCharArray();
      switch (GotChars[2])
      {
        case 'R':
          int? revVbuf = GotChars[3] switch
          {
            'F' => 1,
            'N' => 0,
            'R' => -1,
            'B' => -1,
            _ => null
          };
          if (revVbuf != null) ReverserNum = revVbuf ?? 0;
          break;
        case 'K':
          int? KNum = UFunc.String2INT(GotStr.Substring(3));
          if (KNum == null) break;//値なし
          int[] ka = GIPI.GetBtJobNum(KNum ?? 0);
          if (!(ka?.Length > 0)) break;//機能該当なし
          int i = 0;
          switch (GotChars.Last())
          {
            case 'D':
              while (ka?.Length > i)
              {
                CI?.SetIsKeyPushed(ka[i], true);
                i++;
              }
              break;
            case 'U':
              while (ka?.Length > i)
              {
                CI?.SetIsKeyPushed(ka[i], false);
                i++;
              }
              break;
          }
          break;
        default:
          int Num = 0;
          if (!int.TryParse(GotStr.Substring(3), out Num)) break;
          switch (GotChars[2])
          {
            case 'B':
              BrakeNotchNum = Num;
              break;
            case 'P':
              PowerNotchNum = Num;
              break;
            case 'H':
              PowerNotchNum = -Num;
              break;
          }
          break;
      }

      return GotStr;
    }

    /// <summary>ヘッダがTRであるコマンドでの要求を処理する</summary>
    /// <param name="SVc">受信したインスタンス</param>
    /// <param name="GotString">接尾辞が取り除かれたコマンド文字列</param>
    /// <returns>返信すべき文字列(不要時はnull)</returns>
    static private string DataSelTR(IBIDSsv SVc, in string GotString)
    {
      if (SVc == null || string.IsNullOrWhiteSpace(GotString)) return Errors.GetCMD(Errors.ErrorNums.CMD_ERROR);
      string ReturnString = GotString + "X";//接尾辞は入っていないことを期待する.

      //0 1 2 3
      //T R X X
      switch (GotString.ToCharArray()[2])
      {
        case ConstVals.CMD_REVERSER://レバーサー
          int? revVal = GotString.Substring(3) switch
          {
            "R" => -1,
            "N" => 0,
            "F" => 1,
            "-1" => -1,
            "0" => 0,
            "1" => 1,
            _ => null
          };
          if (revVal == null) return Errors.GetCMD(Errors.ErrorNums.ERROR_in_DType_or_DNum);
          else
          {
            ReverserNum = revVal ?? 0;
            return ReturnString + revVal.ToString();
          }
        case ConstVals.CMD_SPoleMC://ワンハンドル
          int sers = 0;
          if (!int.TryParse(GotString.Substring(3), out sers)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);

          int pnn = 0;
          int bnn = 0;
          if (sers > 0) pnn = sers;
          if (sers < 0) bnn = -sers;
          PowerNotchNum = pnn;
          BrakeNotchNum = bnn;
          return ReturnString + "0";
        case ConstVals.CMD_POWER://Pノッチ操作
          int serp = 0;
          if (!int.TryParse(GotString.Substring(3), out serp)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);
          PowerNotchNum = serp;
          return ReturnString + "0";
        case ConstVals.CMD_BREAK://Bノッチ操作
          int serb = 0;
          if (!int.TryParse(GotString.Substring(3), out serb)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);
          BrakeNotchNum = serb;
          return ReturnString + "0";
        case ConstVals.CMD_KeyCtrl://キー操作
          int serk = 0;
          if (!int.TryParse(GotString.Substring(4), out serk)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);
          switch (GotString.Substring(3, 1))
          {
            //udpr
            case "U":
              //if (KyUp(serk)) return ReturnString + "0";
              //else return "TRE8";
              return "TRE3";
            case "D":
              //if (KyDown(serk)) return ReturnString + "0";
              //else return "TRE8";
              return "TRE3";
            case "P":
              if (0 <= serk && serk < 128) 
              {
                CI?.SetIsKeyPushed(serk, true);
                return ReturnString + "0";
              }
              else
              {
                return "TRE2";
              }
            case "R":
              if (0 <= serk && serk < 128) 
              {
                CI?.SetIsKeyPushed(serk, false);
                return ReturnString + "0";
              }
              else
              {
                return "TRE2";
              }
            default:
              return "TRE3";//記号部不正
          }
        case ConstVals.CMD_INFOREQ://情報取得
          char dtyp = '\0';
          int? dnum = null;

          //dtyp convert
          try
          {
            dtyp = GotString.ToCharArray()[ConstVals.DTYPE_CHAR_POS];
          }
          catch (Exception)
          {
            return Errors.GetCMD(Errors.ErrorNums.DTYPE_ERROR);
          }

          //dnum conv
          try
          {
            dnum = UFunc.String2INT(GotString.Substring(ConstVals.DNUM_POS));
          }
          catch (Exception)
          {
            return Errors.GetCMD(Errors.ErrorNums.DNUM_ERROR);
          }

          if (dnum == null) return Errors.GetCMD(Errors.ErrorNums.DNUM_ERROR);

          string s_ir = null;
          return Get_TRI_Data(out s_ir, dtyp, dnum ?? 0) ? ReturnString + s_ir : s_ir;
        case ConstVals.CMD_AUTOSEND_ADD://Auto Send Add
          int sera = 0;
          if (!int.TryParse(GotString.Substring(4), out sera)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);

          char dtypc_a = GotString.Substring(3, 1).ToCharArray()[0];
          ASList asl = null;
          switch (dtypc_a)
          {
            case ConstVals.DTYPE_PANEL:
            case ConstVals.DTYPE_PANEL_ARR:
              if (PDAutoList == null) PDAutoList = new ASList();
              asl = PDAutoList;
              break;
            case ConstVals.DTYPE_SOUND:
            case ConstVals.DTYPE_SOUND_ARR:
              if (SDAutoList == null) SDAutoList = new ASList();
              asl = SDAutoList;
              break;
            default:
              if (AutoNumL == null) AutoNumL = new ASList();
              asl = AutoNumL;
              break;
          }
          string asres = string.Empty;
          try
          {
            if (!Get_TRI_Data(out asres, dtypc_a, sera, true)) asres = "0";//BIDS SMem疎通状況によらず初期値を取得する.
          }
          catch (Exception)
          {
            return Errors.GetCMD(Errors.ErrorNums.UnknownError);
          }

          if (string.IsNullOrWhiteSpace(asres)) return Errors.GetCMD(Errors.ErrorNums.AS_AddErr);

          if (!asl.Contains(SVc, sera, dtypc_a)) asl.Add(SVc, sera, dtypc_a);

          return ReturnString + asres;
          
        case ConstVals.CMD_AUTOSEND_DEL://Auto Send Delete
          int serd;
          if (!int.TryParse(GotString.Substring(4), out serd)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);

          char dtypc_d = GotString.Substring(3, 1).ToCharArray()[0];
          ASList asld = null;
          ReturnString += "0";//値を入れて返すことはないから, 先に0を付けとく.
          switch (dtypc_d)//nullなら削除する要素は存在しない.
          {
            case ConstVals.DTYPE_PANEL:
            case ConstVals.DTYPE_PANEL_ARR:
              if (PDAutoList == null) return ReturnString;
              else asld = PDAutoList;
              break;
            case ConstVals.DTYPE_SOUND:
            case ConstVals.DTYPE_SOUND_ARR:
              if (SDAutoList == null) return ReturnString;
              else asld = SDAutoList;
              break;
            default:
              if (AutoNumL == null) return ReturnString;
              else asld = AutoNumL;
              break;
          }

          asld.Remove(SVc, serd, dtypc_d);
          return ReturnString;
          
        case ConstVals.CMD_ERROR:
          Console.WriteLine("BIDSsv.Common.DataSelTR : Error ({0}) got from {1}", GotString, SVc.Name);
          return null;

        case ConstVals.CMD_VERSION:
          int serv;
          if (!int.TryParse(GotString.Substring(3), out serv)) return Errors.GetCMD(Errors.ErrorNums.Num_Parsing_Failed);
          if (serv < 0) Errors.GetCMD(Errors.ErrorNums.CMD_ERROR);
          SVc.Version = Math.Min(serv, Version);//デバイスのバージョンとBIDSsvのバージョンを比較し, より小さい値をその通信インスタンスでの採用バージョンとする.
          return ReturnString + SVc.Version.ToString();

        default:
          return "TRE4";//識別子不正
      }
    }

    /// <summary>TRIで始まるコマンドに対応するデータを返す</summary>
    /// <param name="ReturnStr">コマンドに対応するデータを格納する場所(先頭のセパレータは含まれません.)</param>
    /// <param name="DType">要求されたデータのタイプ</param>
    /// <param name="DNum">要求されたデータの番号</param>
    /// <param name="GetDataAnyway">BIDS SMemの疎通状況に依らずにデータを取得するか否か</param>
    /// <param name="separator">使用するセパレータ文字</param>
    /// <returns>エラー番号(なければnull)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    static public bool Get_TRI_Data(out string ReturnStr, in char DType, in int DNum, in bool GetDataAnyway = false, in char separator = ConstVals.CMD_SEPARATOR)
    {
      StringBuilder sb = new StringBuilder();
      bool ret = Get_TRI_Data(sb, DType, DNum, GetDataAnyway, separator);
      ReturnStr = sb.ToString();
			return ret;
    }

    /// <summary>要求された情報を取得する</summary>
    /// <param name="sv">IBIDSsvインスタンス</param>
    /// <param name="str">入力されたコマンド文字列</param>
    /// <returns>返信すべき文字列</returns>
    static public string DataSelect(IBIDSsv sv, in string str)
    {
      if (sv == null) throw new ArgumentNullException();
      if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException();

      if (str.StartsWith(ConstVals.CMD_HEADER))
        if (str.Contains(ConstVals.CMD_SEPARATOR))//データ付き
        {
          DataGot(str);
          return null;
        }
        else return DataSelTR(sv, str);//データ要求
      
      else if (str.StartsWith(ConstVals.GIPI_HEADER)) return DataSelTO(str);

      return null;//対応しないコマンド
    }

    /// <summary>必要なデータを選択すると同時に, 送信まで行う.</summary>
    /// <param name="sv">IBIDSsvインスタンス</param>
    /// <param name="str">入力されたコマンド文字列</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    static public void DataSelSend(IBIDSsv sv, in string str) => sv?.Print(DataSelect(sv, str));

		#endregion

		#region StringBuilderを使用したMethod群
    /// <summary>TRIで始まるコマンドに対するデータを取得します.</summary>
    /// <param name="ReturnStrB"></param>
    /// <param name="DType"></param>
    /// <param name="DNum"></param>
    /// <param name="GetDataAnyway"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    static public bool Get_TRI_Data(StringBuilder ReturnStrB, in char DType, in int DNum, in bool GetDataAnyway = false, in char separator = ConstVals.CMD_SEPARATOR)
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
      string ArrayPrinter<T>(StringBuilder sb,in IList<T> arr, in char separ)
      {
        sb.Append(arr[0]);
        for (int i = 1; i < arr.Count; i++)
          sb.Append(separ).Append(arr[i]);
        return sb.ToString();
      }
      //データ強制取得F && SMem未疎通 => NotConnected
      //データ強制取得F && SMem疎通済 => 値を返す
      //データ強制取得T && (疎通不依存) => 値を返す
      if (!GetDataAnyway && !BSMD.IsEnabled)
      {
        ReturnStrB.Clear();
        ReturnStrB.Append(Errors.GetCMD(Errors.ErrorNums.BIDS_Not_Connected));
        return false;
      }

      #region データ取得部
      object gotD = DataPicker(DType, DNum);
      if (gotD is int || gotD is float || gotD is double od) ReturnStrB.Append(gotD.ToString());
      else if (gotD is Spec spec) ReturnStrB.Append(spec.B).Append(separator).Append(spec.P).Append(separator).Append(spec.A).Append(separator).Append(spec.J).Append(separator).Append(spec.C).ToString();
      else if (gotD is TimeSpan ts) ReturnStrB.Append(ts.Hours).Append(':').Append(ts.Minutes).Append(':').Append(ts.Seconds).Append('.').Append(ts.Milliseconds).ToString();
      else if (gotD is State state) ReturnStrB.Append(state.Z).Append(separator).Append(state.V).Append(separator).Append(state.T).Append(separator).Append(state.BC).Append(separator).Append(state.MR).Append(separator).Append(state.ER).Append(separator).Append(state.BP).Append(separator).Append(state.SAP).Append(separator).Append(state.I).Append(separator).Append(0).ToString();
      else if (gotD is Hand hand) ReturnStrB.Append(hand.B).Append(separator).Append(hand.P).Append(separator).Append(hand.R).Append(separator).Append(hand.C).ToString();
      else if (gotD is float[] farr) ArrayPrinter(ReturnStrB, farr, separator);
      else if (gotD is int[] iarr) ArrayPrinter(ReturnStrB, iarr, separator);
      else
      {
        ReturnStrB.Clear();
        ReturnStrB.Append(Errors.GetCMD(Errors.ErrorNums.ERROR_in_DType_or_DNum));
      }
      #endregion

      return true;
    }

    /// <summary>受信したコマンドにより, 適切な操作を行います.</summary>
    /// <param name="sv"></param>
    /// <param name="str"></param>
    /// <returns></returns>
    static public StringBuilder DataSelect(IBIDSsv sv, StringBuilder str)
    {
      throw new NotImplementedException();
    }

    /// <summary>受信したコマンドにより, 適切な操作を行い, 必要があれば結果を自動で返信します.</summary>
    /// <param name="sv"></param>
    /// <param name="str"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    static public void DataSelSend(IBIDSsv sv, StringBuilder str) => sv?.Print(DataSelect(sv, str).ToString());

    #endregion
  }
}
