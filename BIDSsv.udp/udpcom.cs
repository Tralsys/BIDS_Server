using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Linq;

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

      if (!dsb.IsAlready(ba))
      {
        ba = dsb.RemoveTimeStamp(ba);

        if (IsDebugging)
          Console.WriteLine("udpcom class <<< {0} : {1}", remIPE, ba);

        if (ba?.Length > 0)
          //DataGotEv?.BeginInvoke(remIPE, new UDPGotEvArgs(ba), DataGotEvDoneCallback, DataGotEv);
          DataGotEv?.Invoke(remIPE, new UDPGotEvArgs(ba));
      }
      else if (IsDebugging)
        Console.WriteLine("udpcom class <<<Already<<< {0} : {1}", remIPE, ba);
      ((UdpClient)ar.AsyncState)?.BeginReceive(ReceiveCallback, ar.AsyncState);
    }

    private void DataGotEvDoneCallback(IAsyncResult ar) => ((EventHandler<UDPGotEvArgs>)ar.AsyncState)?.EndInvoke(ar);
    
    public bool DataSend(in byte[] ba)
    {
      byte[] tsba = dsb.AddTimeStamp(ba);
      dsb.SetData(tsba);
      if (UCW?.Client.Connected == true && tsba?.Length > 0) UCW?.BeginSend(tsba, tsba.Length, SendedCallback, UCW);
      else
      {
        if (IsDebugging)
          Console.WriteLine("udpcom class >>> failed.");
        return false;
      }

      if (IsDebugging)
        Console.WriteLine("udpcom class >>> {0} : {1}", UCW.Client.RemoteEndPoint, ba);

      return true;
    }

    private void SendedCallback(IAsyncResult ar) => ((UdpClient)ar.AsyncState).EndSend(ar);
    

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
    const int RecMax = 100;
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

    internal byte[] AddTimeStamp(in byte[] ba)
    {
      if (ba == null || ba.Length <= 0) return null;
      byte[] tsba = new byte[ba.Length + sizeof(int)];

      Array.Copy(ba, 0, tsba, sizeof(int), ba.Length);

      DateTime dt = DateTime.Now;
      int sms = dt.Second * 1000 + dt.Millisecond;
      Array.Copy(sms.GetBytes(), 0, tsba, 0, sizeof(int));

      return tsba;
    }

    internal byte[] RemoveTimeStamp(in byte[] tsba)
    {
      if (tsba == null || tsba.Length <= sizeof(int)) return null;
      byte[] ba = new byte[tsba.Length - sizeof(int)];

      Array.Copy(tsba, sizeof(int), ba, 0, ba.Length);

      return ba;
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
        if (ba.SequenceEqual(Hashes[i])) isSame = true; ;
      });
      return isSame;
    }
  }
}
