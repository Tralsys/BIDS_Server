using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace TR.BIDSsv
{
	public class udp : IBIDSsv
	{
		public event EventHandler<DataGotEventArgs>? DataGot;

		public bool IsDisposed { get => disposedValue; }
		public int Version { get; set; } = 202;
		public string Name { get; private set; } = "udp";
		public bool IsDebug {
			get => isDbg;
			set
			{
				isDbg = value;
				if (udpc != null) udpc.IsDebugging = value;
			}

		}
		private bool isDbg = false;
		Encoding Enc = Encoding.Default;
		udpcom udpc = null;
		public bool Connect(in string args)
		{
			string[] sa = args.Replace(" ", string.Empty).Split(new string[2] { "-", "/" }, StringSplitOptions.RemoveEmptyEntries);
			IPAddress rip = IPAddress.Broadcast;
			IPAddress lip = IPAddress.Any;
			ushort rport = (ushort)Common.DefPNum;
			ushort lport = (ushort)Common.DefPNum;
			for (int i = 0; i < sa.Length; i++)
			{
				string[] saa = sa[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				try
				{
					if (saa?.Length > 0)
					{
						switch (saa[0])
						{
							case "E":
								switch (int.Parse(saa[1]))
								{
									case 0:
										Enc = Encoding.Default;
										break;
									case 1:
										Enc = Encoding.ASCII;
										break;
									case 2:
										Enc = Encoding.Unicode;
										break;
									case 3:
										Enc = Encoding.UTF8;
										break;
									case 4:
										Enc = Encoding.UTF32;
										break;
									default:
										Enc = Encoding.Default;
										break;
								}
								break;
							case "Encoding":
								switch (int.Parse(saa[1]))
								{
									case 0:
										Enc = Encoding.Default;
										break;
									case 1:
										Enc = Encoding.ASCII;
										break;
									case 2:
										Enc = Encoding.Unicode;
										break;
									case 3:
										Enc = Encoding.UTF8;
										break;
									case 4:
										Enc = Encoding.UTF32;
										break;
									default:
										Enc = Encoding.Default;
										break;
								}
								break;
							case "N":
								if (saa.Length > 1) Name = saa[1];
								break;
							case "Name":
								if (saa.Length > 1) Name = saa[1];
								break;
							case "RIP":
								if (saa.Length > 1) rip = IPAddress.Parse(saa[1]);
								break;
							case "RemoteIP":
								if (saa.Length > 1) rip = IPAddress.Parse(saa[1]);
								break;
							case "LIP":
								if (saa.Length > 1) lip = IPAddress.Parse(saa[1]);
								break;
							case "LocalIP":
								if (saa.Length > 1) lip = IPAddress.Parse(saa[1]);
								break;
							case "RPort":
								if (saa.Length > 1) rport = ushort.Parse(saa[1]);
								break;
							case "LPort":
								if (saa.Length > 1) lport = ushort.Parse(saa[1]);
								break;
						}
					}
				}
				catch (Exception e) { Console.WriteLine("Error has occured on " + Name); Console.WriteLine(e); }
			}
			Console.WriteLine("{0} : Connect try ... read_on>{1}:{2} send_to>{3}:{4}", Name, lip, lport, rip, rport);
			udpc = new udpcom(new IPEndPoint(lip, lport), new IPEndPoint(rip, rport));
			udpc.DataGotEv += Udpc_DataGotEv;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		private void Udpc_DataGotEv(object sender, UDPGotEvArgs e)
		{
			if (e.DataLen > 0)
				DataGot?.Invoke(this, new(e.Data));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void Print(in string data)
		{
			if (IsDebug) Console.WriteLine("{0} >> {1}", Name, data);
			if (string.IsNullOrWhiteSpace(data)) return;

			try
			{
				Print(Enc.GetBytes(data + "\n"));
			}
			catch(Exception e)
			{
				Console.WriteLine("{0} : {1}", Name, e);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void Print(in byte[] data) => udpc.DataSend(data);

		readonly string[] ArgInfo = new string[]
		{
			"Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
			"  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code.",
			"  -N or -Name : Set the Instance Name.  Default:\"udp\"  If you don't set this option, this program maybe cause some bugs.",
			"  -RIP or -RemoteIP : Set the IP Address to communicate with.  If you don't set this option, this program starts broadcast communication.",
			"  -LIP or -LocalIP : Set the IP Address to send from.",
			"  -RPort : Set the Remote Port Number to connect.  If you don't set this option, this program uses \"" + Common.DefPNum.ToString() + "\" (default port)",
			"  -LPort : Set the Local Port Number to read.  If you don't set this option, this program uses \"" + Common.DefPNum.ToString() + "\" (default port)"
		};
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void WriteHelp(in string args)
		{
			Console.WriteLine("BIDS Server Program UDP Interface");
			Console.WriteLine("Version : " + Version.ToString());
			Console.WriteLine("Copyright (C) Tetsu Otter 2019");
			foreach (string s in ArgInfo) Console.WriteLine(s);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void OnBSMDChanged(in BIDSSharedMemoryData data) { }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void OnOpenDChanged(in OpenD data) { }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void OnPanelDChanged(in int[] data) { }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void OnSoundDChanged(in int[] data) { }

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
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
				udpc?.Dispose();
				disposedValue = true;
			}
		}

		// TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
		// ~udp()
		// {
		//   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
		//   Dispose(false);
		// }

		// このコードは、破棄可能なパターンを正しく実装できるように追加されました。
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void Dispose()
		{
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			Dispose(true);
			// TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
