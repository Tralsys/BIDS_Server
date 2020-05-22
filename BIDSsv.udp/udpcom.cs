using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
  internal class udpcom : IDisposable
  {
    public event EventHandler<UDPGotEvArgs> DataGotEv;
    public bool IsDebugging { get; set; } = false;
    public uint MyID { get; } = 0;
    UdpClient UCW = null;
    UdpClient UCR = null;
    public udpcom(IPEndPoint Src = null, IPEndPoint Dst = null)
    {
      #region GetMyIP
      IPAddress[] myip = Dns.GetHostAddresses(Dns.GetHostName());
      if (myip?.Length > 0)
      {
        byte[] adrs = myip[0].GetAddressBytes();
        for (int i = 0; i <adrs.Length; i++)
          MyID ^= (MyID << 8) + adrs[i];
      }
      #endregion

      int pt = Src?.Port ?? Common.DefPNum;

      UCR = new UdpClient(Src ?? new IPEndPoint(IPAddress.Any, pt));
      UCW = new UdpClient(0);


      UCR.EnableBroadcast = UCW.EnableBroadcast = true;

      //UCR.BeginReceive(ReceiveCallback, UCR);
      Task.Run(ReadingMethod);
      IPEndPoint ipep = Dst ?? new IPEndPoint(IPAddress.Broadcast, pt);
      UCW.Connect(ipep);

      if (MyID == 0)
      {
        byte[] ucwlep = ((IPEndPoint)UCW.Client.LocalEndPoint).Address.GetAddressBytes();
        MyID = ucwlep[ucwlep.Length - 1];
      }
    }
    private void ReceiveCallback(IAsyncResult ar)
    {
      IPEndPoint remIPE = new IPEndPoint(IPAddress.Any, 0);
      byte[] ba = new byte[0];
      try
      {
        ba = ((UdpClient)ar.AsyncState)?.EndReceive(ar, ref remIPE);
        if (disposing) return;
        Task.Run(() => ((UdpClient)ar.AsyncState)?.BeginReceive(ReceiveCallback, ar.AsyncState));
      }
      catch (ObjectDisposedException) { Console.WriteLine("udpcom ::: ReceiveCallback => ObjectDisposed."); return; }
      catch (Exception) { throw; }
      DataReceived(ba, remIPE);
    }

    private void ReadingMethod()
    {
      IPEndPoint ipEP = new IPEndPoint(IPAddress.Any, 0);
      try
      {
        byte[] ba = UCR?.Receive(ref ipEP);
        if (disposing) return;
        Task.Run(ReadingMethod);
        DataReceived(ba, ipEP);
      }
      catch (ObjectDisposedException)
      {
        Console.WriteLine("udpcom ::: ReadingMethod => ObjectDisposed.");
      }
    }

     private void DataReceived(in byte[] ba, in IPEndPoint remIPE)
    {
      byte[] rba = new byte[0];
      if (IsMyDataCheck(ba, out rba))// || Equals(remIPE.Address, ((IPEndPoint)UCW.Client.LocalEndPoint).Address))
      {
        if (IsDebugging)
          Console.WriteLine("udpcom class <<<<<<My Data<<<<<< {0} (from {2}) : {1}", remIPE, BitConverter.ToString(ba), ((IPEndPoint)UCW.Client.LocalEndPoint));
      }
      else
      {
        if (IsDebugging)
          Console.WriteLine("udpcom class <<< {0} : {1}", remIPE, BitConverter.ToString(rba));

        if (rba?.Length > 0)
          Task.Run(() => DataGotEv?.Invoke(null, new UDPGotEvArgs(rba)));
      }
    }

    public bool DataSend(in byte[] ba)
    {
      if (disposing || !(ba?.Length > 0)) return false;
      byte[] tsba = SetMyID(ba);
      if (UCW?.Client.Connected == true && tsba?.Length > 0) Task.Run(() => UCW?.Send(tsba, tsba.Length));
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
      catch (SocketException) { Console.WriteLine("udpcom sended_callback : socket exception fired."); }
      catch (Exception) { throw; }
    }


    bool IsMyDataCheck(byte[] ba, out byte[] rba)
    {
      rba = new byte[(ba?.Length ?? sizeof(uint)) - sizeof(uint)];
      if (!(ba?.Length > sizeof(uint))) return true;

      if (MyID == UFunc.GetUInt(ba, 0)) return true;

      Buffer.BlockCopy(ba, sizeof(uint), rba, 0, rba.Length);
      return false;
    }

    byte[] SetMyID(byte[] ba)
    {
      if (!(ba?.Length > 0)) return null;
      byte[] rba = new byte[ba.Length + sizeof(uint)];

      Buffer.BlockCopy(MyID.GetBytes(), 0, rba, 0, sizeof(uint));
      Buffer.BlockCopy(ba, 0, rba, sizeof(uint), ba.Length);

      return rba;
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
    internal long DataLen => Data?.LongLength ?? 0;
  }
}
