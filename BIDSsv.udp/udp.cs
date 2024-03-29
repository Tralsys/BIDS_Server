﻿using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
	public class udp : IBIDSsv
	{
		public event EventHandler<DataGotEventArgs>? DataGot;
		public event EventHandler? Disposed;

		public bool IsDisposed { get => disposedValue; }
		public int Version { get; set; } = 202;
		public string Name { get; private set; } = "udp";
		public bool IsDebug
		{
			get => isDbg;
			set
			{
				isDbg = value;
				if (udpc != null) udpc.IsDebugging = value;
			}

		}
		private bool isDbg = false;
		Encoding Enc = Encoding.Default;
		udpcom? udpc = null;
		public bool Connect(in string args)
		{
			string[] sa = args.Replace(" ", string.Empty).Split(new string[2] { "-", "/" }, StringSplitOptions.RemoveEmptyEntries);
			IPAddress? sendToIP = null;
			IPAddress? sendFromIP = null;
			ushort sendToPort = (ushort)ConstVals.DefPNum;
			ushort readOnPort = (ushort)ConstVals.DefPNum;
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
							case "Encoding":
								Enc = int.Parse(saa[1]) switch
								{
									0 => Encoding.Default,
									1 => Encoding.ASCII,
									2 => Encoding.Unicode,
									3 => Encoding.UTF8,
									4 => Encoding.UTF32,
									_ => Encoding.Default,
								};
								break;
							case "N":
							case "Name":
								if (saa.Length > 1) Name = saa[1];
								break;

							case "DstIP":
							case "ToIP":
							case "SendToIP":
								if (saa.Length > 1) sendToIP = IPAddress.Parse(saa[1]);
								break;

							case "SrcIP":
							case "SendFromIP":
								if (saa.Length > 1) sendFromIP = IPAddress.Parse(saa[1]);
								break;

							case "DstPort":
							case "ToPort":
							case "SendToPort":
								if (saa.Length > 1) sendToPort = ushort.Parse(saa[1]);
								break;

							case "ReadPort":
							case "ReadOnPort":
								if (saa.Length > 1) readOnPort = ushort.Parse(saa[1]);
								break;
						}
					}
				}
				catch (Exception e)
				{
					Log(e);
				}
			}

			udpc = new udpcom(sendFromIP, sendToIP, sendToPort, readOnPort);

			udpc.DataGotEv += Udpc_DataGotEv;
			return true;
		}

		private void Udpc_DataGotEv(object? sender, UDPGotEvArgs e)
		{
			if (e.DataLen > 0)
				DataGot?.Invoke(this, new(e.Data));
		}

		public void Print(in string data)
		{
			if (IsDebug)
				Log($">> {data}");

			if (string.IsNullOrWhiteSpace(data)) return;

			try
			{
				Print(Enc.GetBytes(data + "\n"));
			}
			catch (Exception e)
			{
				Log(e);
			}
		}

		public void Print(in byte[] data) => udpc?.DataSend(data);

		readonly string[] ArgInfo = new string[]
		{
			"Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
			"  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code.",
			"  -N or -Name : Set the Instance Name.  Default:\"udp\"  If you don't set this option, this program maybe cause some bugs.",
			"  -DstIP or -SendToIP : Set the IP Address to communicate with.  If you don't set this option, this program starts broadcast communication.",
			"  -SrcIP or -SendFromIP : Set the IP Address to send from.",
			$"  -DstPort or -SendToPort : Set the Remote Port Number to connect.  If you don't set this option, this program uses \"{ConstVals.DefPNum}\" (default port)",
			$"  -ReadOnPort : Set the Local Port Number to read.  If you don't set this option, this program uses \"{ConstVals.DefPNum}\" (default port)"
		};

		public void WriteHelp(in string args)
		{
			Console.WriteLine("BIDS Server Program UDP Interface");
			Console.WriteLine("Version : " + Version.ToString());
			Console.WriteLine("Copyright (C) Tetsu Otter 2019");
			foreach (string s in ArgInfo) Console.WriteLine(s);
		}

		public void OnBSMDChanged(in BIDSSharedMemoryData data) { }

		public void OnOpenDChanged(in OpenD data) { }

		public void OnPanelDChanged(in int[] data) { }

		public void OnSoundDChanged(in int[] data) { }

		private void Log(object obj, [CallerMemberName] string? memberName = null)
			=> Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffff}]({Name}.{memberName}): {obj}");

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}

				udpc?.Dispose();
				disposedValue = true;
				Disposed?.Invoke(this, new());
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
