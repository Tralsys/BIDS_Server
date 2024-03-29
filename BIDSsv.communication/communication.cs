﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
	public class communication : IBIDSsv
	{
		public event EventHandler<DataGotEventArgs>? DataGot;
		public event EventHandler? Disposed;

		const int DefPNum = 9032;
		public bool IsDisposed { get; private set; } = false;
		private int PortNum = DefPNum;
		private int RemotePNum = DefPNum;
		IPAddress? Addr = IPAddress.Any;
		public int Version { get; set; } = 100;
		public string Name { get; private set; } = "communication";
		public bool IsDebug { get; set; } = false;
		bool IsWriteable = false;
		UdpClient? UC = null;

		public bool Connect(in string args)
		{
			string[] sa = args.Replace(" ", string.Empty).Split(new string[2] { "-", "/" }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < sa.Length; i++)
			{
				string[] saa = sa[i].Split(':');
				try
				{
					if (saa.Length > 0)
					{
						switch (saa[0])
						{
							case "A":
							case "Addr":
								if (saa.Length > 1) IsWriteable = IPAddress.TryParse(saa[1], out Addr);
								break;

							case "N":
							case "Name":
								if (saa.Length > 1) Name = saa[1];
								break;

							case "P":
							case "Port":
								if (saa.Length > 1) PortNum = int.Parse(saa[1]);
								break;

							case "RemotePort":
							case "RP":
								if (saa.Length > 1) RemotePNum = int.Parse(saa[1]);
								break;
						}
					}
				}
				catch (Exception e)
				{
					Log($"Error has occured.\n{e}");
				}
			}
			try
			{
				UC = new UdpClient(new IPEndPoint(IPAddress.Any, PortNum));

				if (Addr is not null && Addr != IPAddress.Any)
					UC?.Connect(Addr, RemotePNum);
			}
			catch (Exception e)
			{
				Log($"Error has occured.\n{e}");
				return false;
			}

			UC?.BeginReceive(ReceivedDoing, UC);
			return true;
		}

		SocketException? sexc = null;
		private void ReceivedDoing(IAsyncResult ar)
		{
			if (ar.AsyncState is not UdpClient uc)
				return;

			try
			{
				IPEndPoint? ipep = null;
				byte[] rba = uc.EndReceive(ar, ref ipep);

				DataGot?.Invoke(this, new(rba));
			}
			catch (SocketException e)
			{
				if (!Equals(sexc?.ErrorCode, e.ErrorCode))
					Log($"Receieve Error({e.ErrorCode}) => {e}");

				sexc = e;
			}
			catch (ObjectDisposedException)
			{
				Log("This connection is already closed.");
				Dispose();
				return;
			}

			uc.BeginReceive(ReceivedDoing, uc);
		}

		protected virtual void Dispose(bool tf)
		{
			if (!tf)
				UC?.Close();
			UC?.Dispose();
			IsDisposed = true;
			Disposed?.Invoke(this, new());
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		readonly int[] PDA = new int[256];
		readonly int[] SDA = new int[256];
		int oldT = 0;
		public void OnBSMDChanged(in BIDSSharedMemoryData data)
		{
			if (!IsWriteable) return;
			UC?.Send(
				CommunicationDllConverter.CommunicationBAGet(
					CommunicationDllConverter.BSMDtoComStr(data, data.StateData.T - oldT),
					PDA,
					SDA),
				CommunicationDllConverter.ComStrSize + (CommunicationDllConverter.PSArrSize * 2)
			);

			oldT = data.StateData.T;
		}

		public void OnOpenDChanged(in OpenD data) { }

		public void OnPanelDChanged(in int[] data)
		{
			if (!IsWriteable) return;
			Buffer.BlockCopy(data, 0, PDA, 0, Math.Min(256, data.Length) * sizeof(int));
			if (data.Length < 256) Array.Clear(PDA, data.Length, 256 - data.Length);
		}

		public void OnSoundDChanged(in int[] data)
		{
			if (!IsWriteable) return;
			Buffer.BlockCopy(data, 0, SDA, 0, Math.Min(256, data.Length) * sizeof(int));
			if (data.Length < 256) Array.Clear(SDA, data.Length, 256 - data.Length);
		}

		public void Print(in string data) => Print(Encoding.Default.GetBytes(data));

		public void Print(in byte[] data)
		{
			if (IsWriteable && UC != null && data?.Length > 0)
				UC.Send(data, data.Length);
		}

		readonly string[] ArgInfo = new string[]
		{
			"Argument Format ... [Header(\"-\" or \"/\") + SettingName(B, P etc...) + Separater(\":\") + Setting(38400, 2 etc...)]",
			"  -A or -Address : Set the Address to send to.  If you don't set this option, this program will run as a Reader Only.",
			"  -P or -Port : Set the Sending(Local) Port.  If you don't set this option, this program uses \"9032\" as the Local Port Number.",
			"  -RP or -RemotePort : Set the Remote Port.  If you don't set this option, this program uses \"9032\" as the Remote Port Number.",
			"  -N or -Name : Set the Connection Name."

		};
		public void WriteHelp(in string args)
		{
			Console.WriteLine("BIDS Server Program Communication.dll Interface");
			Console.WriteLine("Version : " + Version.ToString());
			Console.WriteLine("Copyright (C) Tetsu Otter 2019");
			foreach (string s in ArgInfo) Console.WriteLine(s);
		}

		private void Log(object obj, [CallerMemberName] string? memberName = null)
			=> Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffff}]({Name}.{memberName}): {obj}");
	}
}
