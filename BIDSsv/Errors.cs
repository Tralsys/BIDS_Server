using System;
using System.Collections.Generic;
using System.Text;

namespace TR.BIDSsv
{
  public static class Errors
  {
    public enum ErrorNums
    {
      UnknownError,
      BIDS_Not_Connected,
      DTYPE_ERROR,
      DNUM_ERROR,
      CMD_ERROR,
      Parsing_overflow,
      Char_in_DNum,
      ERROR_in_DType_or_DNum,
      Cannot_get_BVE_WindowHandle,
      AS_AddErr
    }

    public static string GetCMD(this ErrorNums ErrNum) => ConstVals.CMD_HEADER + ConstVals.CMD_ERROR + ((int)ErrNum).ToString();
    
  }

  /// <summary>BIDSのコマンド周りのエラーを報告する</summary>
  public class BIDSErrException : Exception
  {
    public Errors.ErrorNums ErrNum { get; }
    public string ErrCMD { get; }

    public override string Message { get; }

    public BIDSErrException(Errors.ErrorNums errnum)
    {
      ErrNum = errnum;
      ErrCMD = Errors.GetCMD(ErrNum);
      Message = string.Format("{0} ({1})", ErrCMD, ErrNum);
    }
  }
}
