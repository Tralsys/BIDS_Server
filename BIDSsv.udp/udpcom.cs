using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
	internal class udpcom : IDisposable
	{
		public event EventHandler<UDPGotEvArgs>? DataGotEv;
		public bool IsDebugging { get; set; } = false;
		readonly UdpClient UCW;
		readonly UdpClient UCR;
		readonly IPEndPoint? MyIPEndPoint = null;
		readonly CancellationTokenSource ReaderCancellationTokenSrc = new();
		readonly CancellationTokenSource WriterCancellationTokenSrc = new();
		const ushort SendFromPort = 0;

		public udpcom(IPAddress? SendFromIP, IPAddress? SendToIP, ushort SendToPort, ushort ReadOnPort)
		{
			if (SendFromIP is null || Equals(SendFromIP, IPAddress.Any))
			{
				foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)//IPv4 only
					{
						// ループバックアドレスは使用しない
						if (ip.GetAddressBytes()[0] == 127)
							continue;

						MyIPEndPoint = new(ip, SendFromPort);
						MyIPEndPoint.Address = ip;
						break;
					}
				}

				if (MyIPEndPoint is null)
					throw new Exception("Cannot set source IP Address");
			}
			else
				MyIPEndPoint = new(SendFromIP, SendFromPort);

			UCR = new UdpClient(new IPEndPoint(IPAddress.Any, ReadOnPort));
			UCW = new UdpClient(MyIPEndPoint);

			UCR.EnableBroadcast = UCW.EnableBroadcast = true;

			_ = Task.Run(ReadingMethod);
			UCW.Connect(new IPEndPoint(SendToIP ?? IPAddress.Broadcast, SendToPort));

			Log($"Reader listening on: {UCR.Client.LocalEndPoint} / Writer print from: {UCW.Client.LocalEndPoint}");
		}

		private async Task ReadingMethod()
		{
			CancellationToken token = ReaderCancellationTokenSrc.Token;

			try
			{
				while (!disposing && !disposedValue && !token.IsCancellationRequested)
				{
					var result = await UCR.ReceiveAsync(token);

					if (disposing)
						return;

					DataReceived(result.Buffer, result.RemoteEndPoint);
				}
			}
			catch (ObjectDisposedException)
			{
				Log("ObjectDisposed.");
			}
			catch (SocketException ex)
			{
				Log($"Some Socket Exception was thrown ... {ex}");
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
				{
					Task.Run(() => DataGotEv?.Invoke(null, new UDPGotEvArgs(ba)));
				}
			}
		}

		public bool DataSend(byte[] ba)
		{
			if (disposing || !(ba?.Length > 0))
				return false;

			if (UCW?.Client.Connected == true && ba?.Length > 0)
			{
				UCW.SendAsync(ba.AsMemory(), WriterCancellationTokenSrc.Token)
					.AsTask()
					.ContinueWith((v) =>
					{
						if (IsDebugging)
							Log($">>> {UCW.Client.RemoteEndPoint} (written {v.Result} bytes): {BitConverter.ToString(ba)}");
					});
			}
			else
			{
				if (IsDebugging)
					Log(">>> failed.");

				return false;
			}

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
					ReaderCancellationTokenSrc.Cancel();
					WriterCancellationTokenSrc.Cancel();

					UCW?.Close();
					UCR?.Close();
				}

				UCW?.Dispose();
				UCR?.Dispose();

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
