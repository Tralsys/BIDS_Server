using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
  internal class ASList
  {
    public int Count { get => SvList?.Count ?? 0; }
    private readonly List<IBIDSsv> SvList;//外部からの操作はメソッドを通して. 初期化は最初だけ.
    private readonly List<int> DNums;//外部からの操作はメソッドを通して. 初期化は最初だけ
    private readonly List<char> DTypes;//外部からの操作はメソッドを通して. 初期化は最初だけ.

    /// <summary>リストの操作権を管理します.</summary>
    ReaderWriterLockSlim LLock = null;
    public ASList(bool UseTypeChar = true)
    {
      LLock = new ReaderWriterLockSlim();
      try
      {
        LLock.EnterWriteLock();
        SvList = new List<IBIDSsv>();
        DNums = new List<int>();
        DTypes = UseTypeChar ? new List<char>() : null;
      }
      finally
      {
        if (LLock.IsWriteLockHeld)
          LLock.ExitWriteLock();
      }
    }

    /// <summary>Listに指定の要素が含まれているかを確認します.</summary>
    /// <param name="sv">登録された通信クラスインスタンス</param>
    /// <param name="dnum">データ番号</param>
    /// <param name="dtyp">データタイプ記号</param>
    /// <returns>CountNum以上の</returns>
    public bool Contains(IBIDSsv sv, int dnum, char dtyp = ConstVals.NULL_CHAR)
    {
      bool result = false;
      bool LockGot = false;
      try
      {
        LLock.EnterReadLock();
        LockGot = true;
        if (sv == null || Count <= 0) return false;
        Parallel.For(0, SvList.Count, (i) =>
        {
          if (SvList[i] == sv && (DTypes == null ? true : DTypes[i] == dtyp) && DNums[i] == dnum) result = true;
        });
      }
      finally
      {
        if (LLock.IsReadLockHeld && LockGot)
          LLock.ExitReadLock();
      }
      return result;
    }

    public void Remove(IBIDSsv sv, int? dnum = null, char? dtyp = null)
    {
      bool LockGot = false;
      try
      {
        LLock.EnterUpgradeableReadLock();
        LockGot = true;
        if (Count <= 0) return;
        bool WLockGot = false;
        try
        {
          LLock.EnterWriteLock();
          WLockGot = true;
          for (int i = Count - 1; i >= 0; i--)
          {
            if (SvList[i] == sv)
            {
              if (dtyp != null && dtyp == DTypes[i])//type is specified and it is same
                if (!(dnum == null || DNums[i] == dnum)) continue;//not match (type same / num is specified but not same)
              if (dtyp == null && dnum != DNums[i]) continue;//type not specified and num is not same
              SvList.RemoveAt(i);
              DNums.RemoveAt(i);
              DTypes?.RemoveAt(i);
            }
          }
        }
        finally
        {
          if (LLock.IsWriteLockHeld && WLockGot)
            LLock.ExitWriteLock();
        }
      }
      finally
      {
        if (LLock.IsUpgradeableReadLockHeld && LockGot)
          LLock.ExitUpgradeableReadLock();
      }
    }

    public void Add(IBIDSsv key, int dnum, char dtyp = ConstVals.NULL_CHAR)
    {
      bool LockGot = false;
      try
      {
        LLock.EnterWriteLock();
        LockGot = true;
        SvList.Add(key);
        DTypes?.Add(dtyp);
        DNums.Add(dnum);
      }
      finally
      {
        if (LLock.IsWriteLockHeld && LockGot)
          LLock.ExitWriteLock();
      }
    }

    public List<IBIDSsv> GetSV(int dnum, char dtyp = ConstVals.NULL_CHAR)
    {
      List<IBIDSsv> svl = new List<IBIDSsv>();
      bool LockGot = false;
      object svlLock = new object();
      try
      {
        LLock.EnterReadLock();
        LockGot = true;
        Parallel.For(0, Count, (i) =>
        {
          if (DNums[i] == dnum && (DTypes == null ? true : DTypes[i] == dtyp))
          {
            lock (svlLock)
            {
              svl.Add(SvList[i]);
            }
          }

        });
      }
      finally
      {
        if (LLock.IsReadLockHeld && LockGot)
          LLock.ExitReadLock();
      }
      return svl;
    }

    /// <summary>条件に合致したsvにPrint命令を送出します.</summary>
    /// <param name="printStr">出力する文字列</param>
    /// <param name="dnum">条件とするデータ番号</param>
    /// <param name="dtyp">条件とするタイプ文字</param>
    public void PrintValue(string printStr, int dnum, char dtyp = ConstVals.NULL_CHAR)
    {
      if (string.IsNullOrWhiteSpace(printStr)) return;
      bool LockGot = false;
      try
      {
        LLock.EnterReadLock();
        LockGot = true;
        Parallel.For(0, Count, (i) =>
        {
          if (DNums[i] == dnum && (DTypes == null ? true : DTypes[i] == dtyp))
            Task.Run(() => SvList[i].Print(printStr));
        });
      }
      finally
      {
        if (LLock.IsReadLockHeld && LockGot)
          LLock.ExitReadLock();
      }
    }

    public Elem ElementAt(int index)
    {
      bool LockGot = false;
      try
      {
        LLock.EnterReadLock();
        LockGot = true;
        return new Elem(SvList[index], DNums[index], DTypes?[index]);
      }
      finally
      {
        if (LLock.IsReadLockHeld && LockGot)
          LLock.ExitReadLock();
      }
    }
    public class Elem
    {
      public Elem(IBIDSsv SV, int DN, char? DT)
      {
        sv = SV;
        DNum = DN;
        DType = DT ?? ConstVals.NULL_CHAR;
      }
      public readonly IBIDSsv sv;
      public readonly int DNum;
      public readonly char DType;
    }
  }
}
