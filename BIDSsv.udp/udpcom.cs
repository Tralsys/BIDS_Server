using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
	internal class udpcom : IDisposable
	{
		public event EventHandler<UDPGotEvArgs>? DataGotEv;
		public bool IsDebugging { get; set; } = false;
		UdpClient? UCW = null;
		UdpClient? UCR = null;
		readonly IPEndPoint? MyIPEndPoint = null;

		public udpcom(IPEndPoint? Src = null, IPEndPoint? Dst = null)
		{
			if (Src == null || Equals(Src.Address, IPAddress.Any))
			{
				MyIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
				foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)//IPv4 only
					{
						MyIPEndPoint.Address = ip;
						break;
					}
				}
			}
			else MyIPEndPoint = Src;

			int pt = Src?.Port ?? ConstVals.DefPNum;
			UCR = new UdpClient(Src ?? new IPEndPoint(IPAddress.Any, pt));
			UCW = new UdpClient(MyIPEndPoint);
			IPEndPoint? lep = UCW.Client.LocalEndPoint as IPEndPoint;
			MyIPEndPoint.Port = lep?.Port ?? ConstVals.DefPNum;

			UCR.EnableBroadcast = UCW.EnableBroadcast = true;

			_ = Task.Run(ReadingMethod);
			UCW.Connect(Dst ?? new IPEndPoint(IPAddress.Broadcast, pt));
		}

		private void ReadingMethod()
		{
			IPEndPoint ipEP = new IPEndPoint(IPAddress.Any, 0);
			try
			{
				byte[] ba = UCR?.Receive(ref ipEP) ?? Array.Empty<byte>();
				if (disposing) return;
				_ = Task.Run(ReadingMethod);//再帰的
				DataReceived(ba, ipEP);
			}
			catch (ObjectDisposedException)
			{
				Log("ObjectDisposed.");
			}
		}

		private void DataReceived(byte[] ba, in IPEndPoint remIPE)
		{
			if (Equals(remIPE.Address, MyIPEndPoint?.Address))
			{
				if (IsDebugging)
					Log($"<<<<<<My Data<<<<<< {UCR?.Client.LocalEndPoint as IPEndPoint} (from {remIPE}) : {BitConverter.ToString(ba)}");
			}
			else
			{
				if (IsDebugging)
					Log($"<<< {remIPE} : {BitConverter.ToString(ba)}");

				if (ba?.Length > 0)
					DataGotEv?.Invoke(null, new UDPGotEvArgs(ba));
			}
		}

		public bool DataSend(byte[] ba)
		{
			if (disposing || !(ba?.Length > 0)) return false;
			if (UCW?.Client.Connected == true && ba?.Length > 0) _ = Task.Run(() => _ = UCW?.Send(ba, ba.Length));
			else
			{
				if (IsDebugging)
					Log(">>> failed.");

				return false;
			}

			if (IsDebugging)
				Log($">>> {UCW.Client.RemoteEndPoint} : {BitConverter.ToString(ba)}");

			return true;
		}

		private static void Log(object obj, [CallerMemberName] string? memberName = null)
			=> Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffff}]({nameof(udpcom)}.{memberName}): {obj}");

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには
		private bool disposing = false;

		protected virtual void Dispose(bool disposing)
		{
			this.disposing = true;
			if (!disposedValue)
			{
				if (disposing)
				{
					UCW?.Close();
					UCR?.Close();
				}

				UCW?.Dispose();
				UCR?.Dispose();

				UCW = null;
				UCR = null;
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
