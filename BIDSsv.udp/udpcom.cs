using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
		IPEndPoint MyIPEndPoint = null;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public udpcom(IPEndPoint Src = null, IPEndPoint Dst = null)
		{
			IPAddress myip = IPAddress.Any;
			if (Src == null || Equals(Src.Address, IPAddress.Any))
			{
				foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)//IPv4 only
					{
						myip = ip;
						break;
					}
				}
			}
			else myip = Src.Address;
			byte[] adrs = myip.GetAddressBytes();
			for (int i = 0; i < adrs.Length; i++)
				MyID ^= (MyID << 8) + adrs[i];//ID生成

			int pt = Src?.Port ?? Common.DefPNum;
			MyIPEndPoint = new IPEndPoint(myip, 0);
			UCR = new UdpClient(Src ?? new IPEndPoint(IPAddress.Any, pt));
			UCW = new UdpClient(MyIPEndPoint);
			MyIPEndPoint.Port = ((IPEndPoint)UCW.Client.LocalEndPoint).Port;

			UCR.EnableBroadcast = UCW.EnableBroadcast = true;

			_ = Task.Run(ReadingMethod);
			UCW.Connect(Dst ?? new IPEndPoint(IPAddress.Broadcast, pt));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		private void ReadingMethod()
		{
			IPEndPoint ipEP = new IPEndPoint(IPAddress.Any, 0);
			try
			{
				byte[] ba = UCR?.Receive(ref ipEP);
				if (disposing) return;
				_ = Task.Run(ReadingMethod);
				DataReceived(ba, ipEP);
			}
			catch (ObjectDisposedException)
			{
				Console.WriteLine("udpcom ::: ReadingMethod => ObjectDisposed.");
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		private void DataReceived(in byte[] ba, in IPEndPoint remIPE)
		{
			byte[] rba = new byte[0];
			if (IsMyDataCheck(ba, out rba))
			{
				if (IsDebugging)
					Console.WriteLine("udpcom class <<<<<<My Data<<<<<< {0} (from {2}) : {1}", remIPE, BitConverter.ToString(ba), ((IPEndPoint)UCW.Client.LocalEndPoint));
			}
			else
			{
				if (IsDebugging)
					Console.WriteLine("udpcom class <<< {0} : {1}", remIPE, BitConverter.ToString(rba));

				if (rba?.Length > 0)
					_ = Task.Run(() => DataGotEv?.Invoke(null, new UDPGotEvArgs(rba)));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public bool DataSend(in byte[] ba)
		{
			if (disposing || !(ba?.Length > 0)) return false;
			byte[] tsba = SetMyID(ba);
			if (UCW?.Client.Connected == true && tsba?.Length > 0) _ = Task.Run(() => _ = UCW?.Send(tsba, tsba.Length));
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		bool IsMyDataCheck(byte[] ba, out byte[] rba)
		{
			rba = new byte[(ba?.Length ?? sizeof(uint)) - sizeof(uint)];
			if (!(ba?.Length > sizeof(uint))) return true;

			if (MyID == UFunc.GetUInt(ba, 0)) return true;

			Buffer.BlockCopy(ba, sizeof(uint), rba, 0, rba.Length);
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
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

	internal readonly struct UDPGotEvArgs// : EventArgs
	{
		public UDPGotEvArgs(byte[] ba)
		{
			Data = ba;
		}
		internal byte[] Data { get; }
		internal long DataLen => Data?.LongLength ?? 0;
	}
}
