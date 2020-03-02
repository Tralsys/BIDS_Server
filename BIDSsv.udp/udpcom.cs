using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;

namespace TR.BIDSsv
{
  internal class udpcom : IDisposable
  {
    public event EventHandler<UDPGotEvArgs> DataGotEv;
    public bool IsDebugging { get; set; } = false;
    UdpClient UCW = null;
    UdpClient UCR = null;
    DSendBlock dsb;
    public udpcom(IPEndPoint Src = null, IPEndPoint Dst = null)
    {
      int pt = Src?.Port ?? Common.DefPNum;

      UCR = new UdpClient(Src ?? new IPEndPoint(IPAddress.Any, pt));
      UCW = new UdpClient(0);

      dsb = new DSendBlock();

      UCR.EnableBroadcast = UCW.EnableBroadcast = true;

      UCR.BeginReceive(ReceiveCallback, UCR);

      IPEndPoint ipep = Dst ?? new IPEndPoint(IPAddress.Broadcast, pt);
      UCW.Connect(ipep);
    }
    OldChecker ocr = new OldChecker();
    private void ReceiveCallback(IAsyncResult ar)
    {
      IPEndPoint remIPE = new IPEndPoint(IPAddress.Any, 0);
      byte[] ba = new byte[0];
      try
      {
        ba = ((UdpClient)ar.AsyncState)?.EndReceive(ar, ref remIPE);
      }
      catch (ObjectDisposedException) { Console.WriteLine("udpcom ::: ReceiveCallback => ObjectDisposed."); return; }
      catch (Exception) { throw; }

      if (ocr.IsOldData(remIPE.Address, ba))
      {
        if (IsDebugging)
          Console.WriteLine("udpcom class <<<<<<Old Data<<<<<< {0} : {1}", remIPE, ba);
      }
      else if (!dsb.IsAlready(ba))
      {
        ba = ocr.RemoveTimeStamp(ba);

        if (IsDebugging)
          Console.WriteLine("udpcom class <<< {0} : {1}", remIPE, BitConverter.ToString(ba));

        if (ba?.Length > 0)
          DataGotEv?.Invoke(remIPE, new UDPGotEvArgs(ba));
      }
      else if (IsDebugging)
        Console.WriteLine("udpcom class <<<<<<Already<<<<<< {0} : {1}", remIPE, ba);
      if (disposing) return;
      try
      {
        ((UdpClient)ar.AsyncState)?.BeginReceive(ReceiveCallback, ar.AsyncState);
      }
      catch (ObjectDisposedException) { return; }
      catch (Exception) { throw; }
    }
    
    public bool DataSend(in byte[] ba)
    {
      if (disposing) return false;
      byte[] tsba = ocr.AddTimeStamp(ba);
      dsb.SetData(tsba);
      if (UCW?.Client.Connected == true && tsba?.Length > 0) UCW?.BeginSend(tsba, tsba.Length, SendedCallback, UCW);
      else
      {
        if (IsDebugging)
          Console.WriteLine("udpcom class >>> failed.");
        return false;
      }

      if (IsDebugging)
        Console.WriteLine("udpcom class >>> {0} : {1}", UCW.Client.RemoteEndPoint, BitConverter.ToString(ba));

      return true;
    }

    private void SendedCallback(IAsyncResult ar)
    {
      try
      {
        ((UdpClient)ar.AsyncState).EndSend(ar);
      }
      catch (ObjectDisposedException) { Console.WriteLine("udpcom sended_callback : objectdisposed exception fired."); }
      catch (Exception) { throw; }
    }
    

    #region IDisposable Support
    private bool disposedValue = false; // 重複する呼び出しを検出するには
    private bool disposing = false;

    protected virtual void Dispose(bool disposing)
    {
      disposing = true;
      if (!disposedValue)
      {
        UCW?.Close();
        UCR?.Close();
        if (disposing)
        {
          // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
        }

        // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
        // TODO: 大きなフィールドを null に設定します。
        dsb?.Dispose();
        UCW?.Dispose();
        UCR?.Dispose();

        UCW = null;
        UCR = null;
        disposedValue = true;
      }
    }

    // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
    // ~udpcom()
    // {
    //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
    //   Dispose(false);
    // }

    // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
    public void Dispose()
    {
      // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
      Dispose(true);
      // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
      // GC.SuppressFinalize(this);
    }
    #endregion
  }

  internal class UDPGotEvArgs : EventArgs
  {
    public UDPGotEvArgs(byte[] ba)
    {
      Data = ba;
    }
    internal byte[] Data { get; }
    internal long DataLen
    {
      get
      {
        return Data?.LongLength ?? 0;
      }
    }
  }

  internal class DSendBlock : IDisposable
  {
    FixedSizeList fsl;
    internal DSendBlock()
    {
      fsl = new FixedSizeList();
    }

    internal bool IsAlready(in byte[] ba)
    {
      byte[] hash = GetHash(ba);

      if (!(hash?.Length > 0)) return false;

      return fsl.IsIn(hash);
    }

    internal void SetData(in byte[] ba) => fsl.SetValue(GetHash(ba));

    private byte[] GetHash(in byte[] ba)
    {
      using (var sha = new SHA1CryptoServiceProvider())
      {
        sha.Initialize();
        sha?.TransformFinalBlock(ba, 0, Math.Min(ba.Length, 512));
        return sha?.Hash;
      }
    }
    #region IDisposable Support
    private bool disposedValue = false; // 重複する呼び出しを検出するには

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
        }

        // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
        // TODO: 大きなフィールドを null に設定します。
        disposedValue = true;
      }
    }

    // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
    // ~DSendBlock()
    // {
    //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
    //   Dispose(false);
    // }

    // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
    public void Dispose()
    {
      // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
      Dispose(true);
      // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
      // GC.SuppressFinalize(this);
    }
    #endregion

  }

  internal class FixedSizeList
  {
    const int RecMax = 1000;
    public byte[][] Hashes { get; private set; } = new byte[RecMax][];
    public int Index
    {
      get => index;
      set => index = value % 100;
      
    }
    private int index = 0;

    public void SetValue(byte[] ba)
    {
      if (!(ba?.Length == 20)) return;
      Hashes[Index] = ba;
      Index++;
    }
    
    public bool IsIn(byte[] ba)
    {
      bool isSame = false;
      Parallel.For(0, RecMax, (i) =>
      {
        if (isSame || Hashes[i] == null) return;
        if (StructuralComparisons.StructuralEqualityComparer.Equals(ba, Hashes[i])) isSame = true;
      });
      return isSame;
    }
  }

  internal class OldChecker
  {
    const int TimeThresholdH = int.MaxValue;//40 * 1000 / DSendBlock.DivNum;
    const int TimeThresholdL = 3;//10 / DSendBlock.DivNum;
    //const int DivNum = 2;

    uint Counter = 0;
    List<RecData> rd = new List<RecData>();
    
    internal bool IsOldData(IPAddress ip, in byte[] ba)
    {
      if (ba[4] != 't' || ba[5] != 'r') return false;

      uint time = ba.GetUInt(0);
      byte[] hd = new byte[4];
      Array.Copy(ba, 4, hd, 0, 4);
      if (!(rd?.Count > 0))
      {
        if (rd == null) rd = new List<RecData>();

        rd.Add(new RecData(ip, time, hd));

        return false;
      }
      for(int i = 0; i < rd.Count; i++)
      {
        if (!Equals(rd[i].IP, ip)) continue;

        if (!(rd[i].Header.SequenceEqual(hd))) continue;

        long t = time - rd[i].Time;

        if (t > 0 || t < -TimeThresholdH) rd[i].Time = time;

        return -TimeThresholdH < t && t < -TimeThresholdL;

      }

      rd.Add(new RecData(ip, time, hd));

      return false;
    }
    
    internal byte[] AddTimeStamp(in byte[] ba)
    {
      if (ba == null || ba.Length <= 0) return null;
      byte[] tsba = new byte[ba.Length + sizeof(uint)];

      Array.Copy(ba, 0, tsba, sizeof(uint), ba.Length);

      //DateTime dt = DateTime.Now;
      //int sms = dt.Second * 1000 + dt.Millisecond; sms /= 2;
      uint sms = 0;
      if (Counter > uint.MaxValue - 10000)
        sms = Counter = 0;
      else sms = ++Counter;
      Array.Copy(sms.GetBytes(), 0, tsba, 0, sizeof(uint));

      return tsba;
    }

    internal byte[] RemoveTimeStamp(in byte[] tsba)
    {
      if (tsba == null || tsba.Length <= sizeof(uint)) return null;
      byte[] ba = new byte[tsba.Length - sizeof(uint)];

      Array.Copy(tsba, sizeof(uint), ba, 0, ba.Length);

      return ba;
    }

    internal class RecData
    {
      internal RecData(IPAddress ip, uint time, byte[] header)
      {
        IP = ip;
        Time = time;
        Header = header;
      }

      internal IPAddress IP { get; private set; }
      internal uint Time { get; set; }
      internal byte[] Header { get; private set; }
    }
  }
}
