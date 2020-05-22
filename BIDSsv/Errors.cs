using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
      AS_AddErr,
      Num_Parsing_Failed
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    public static string GetCMD(this ErrorNums ErrNum) => ErrNum.GetCMD_SB().ToString();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    public static StringBuilder GetCMD_SB(this ErrorNums ErrNum) => new StringBuilder(ConstVals.CMD_HEADER_ERROR).Append((int)ErrNum);


  }
}
