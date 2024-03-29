﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
	class TCPsv : IBIDSsv, IBIDSsv.IManager
	{
		public event EventHandler<ControlModEventArgs>? AddMod;
		public event EventHandler<ControlModEventArgs>? RemoveMod;

		public event EventHandler<DataGotEventArgs>? DataGot;
		public event EventHandler? Disposed;

		//Wait Port : 14147
		int PortNum = ConstVals.DefPNum;
		public bool IsDisposed { get; private set; } = false;
		public int Version { get; set; } = 202;

		public string Name { get; private set; } = "tcpsv";
		public bool IsDebug { get; set; } = false;

		TcpListener? TL = null;
		TcpClient? TC = null;
		NetworkStream? NS = null;
		Task? TD = null;

		Encoding Enc = Encoding.Default;
		bool IsLooping = true;
		int WTO = 1000;
		int RTO = 10000;

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
							case "E":
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
							case "P":
							case "Port":
								if (saa.Length > 1) PortNum = int.Parse(saa[1]);
								break;

							case "RTO":
							case "ReadTimeout":
								RTO = int.Parse(saa[1]);
								break;
							case "WTO":
							case "WriteTimeout":
								WTO = int.Parse(saa[1]);
								break;
						}
					}
				}
				catch (Exception e)
				{
					Log(e);
				}
			}
			TL = new TcpListener(IPAddress.Any, PortNum);
			TL.Start();
			Log($"Connection Waiting Start => Addr:{((IPEndPoint)TL.LocalEndpoint).Address}, Port:{((IPEndPoint)TL.LocalEndpoint).Port}");
			TD = PortNum != ConstVals.DefPNum ? new Task(ReadDoing) : new Task(ConnectDoing);

			TD.Start();
			return true;
		}
		public int Connect(Encoding enc, int vnum, int wto = 1000, int rto = 10000)
		{
			Enc = enc;
			WTO = wto;
			RTO = rto;
			Version = vnum;
			TL = new TcpListener(IPAddress.Any, 0);
			TL?.Start();

			IPEndPoint? ep = TL?.LocalEndpoint as IPEndPoint;

			PortNum = ep?.Port ?? ConstVals.DefPNum;
			Log($"Connection Waiting Start => Addr:{ep?.Address}, Port:{PortNum}");

			TD = new Task(ReadDoing);

			return PortNum;
		}

		public void Dispose()
		{
			IsLooping = false;
			if (TD?.IsCompleted == false && TD?.Wait(5000) == false)
				Log("ReadThread is not closed.  It may cause some bugs.");

			NS?.Dispose();
			TC?.Dispose();
			TL?.Stop();
			IsDisposed = true;
			Disposed?.Invoke(this, new());
		}

		async void ConnectDoing()
		{
			while (IsLooping)
			{
				while (IsLooping && TL?.Pending() == false)
					await Task.Delay(1);

				if (!IsLooping)
					continue;

				try
				{
					TC = TL?.AcceptTcpClient();
					NS = TC?.GetStream();
					if (NS is not null)
					{
						NS.ReadTimeout = RTO;
						NS.WriteTimeout = WTO;
					}
				}
				catch (Exception e)
				{
					Log($"ConnectDoing Failed.\n{e}");
				}

				string gs = await Read();

				if (gs.StartsWith("TRV"))//要変更 専用コマンドを用意すること.
				{
					try
					{
						int v = int.Parse(gs.Replace("TRV", string.Empty));
						int vnum = v < Version ? v : Version;
						TCPsv tcp = new TCPsv();
						int pn = tcp.Connect(Enc, vnum, WTO, RTO);
						Print("TRV" + vnum.ToString() + "PN" + pn.ToString() + "\n");
						tcp.Name = "tcp" + pn.ToString();
						AddMod?.Invoke(this, new(tcp));
					}
					catch (Exception e)
					{
						Log($"Version Check Failed.\n{e}");
					}
				}
				else
				{
					Print("TRE\n");//Error を入れる
				}

				NS?.Close();
				NS?.Dispose();
				TC?.Close();
				TC?.Dispose();
			}
		}

		//https://dobon.net/vb/dotnet/internet/tcpclientserver.html
		async void ReadDoing()
		{
			try
			{
				TC = TL?.AcceptTcpClient();
				IPEndPoint? rep = TC?.Client.RemoteEndPoint as IPEndPoint;
				Log($"Connected => Addr:{rep?.Address}, Port:{rep?.Port}");
				NS = TC?.GetStream();
			}
			catch (Exception e)
			{
				Log($"In connection waiting process, An Error has occured\n{e}");
			}

			_ = Task.Run(async () =>
			{
				while (TC?.Connected == true) await Task.Delay(1);
				IsLooping = false;
			});

			while (IsLooping)
			{
				if (TC?.Connected != true) continue;
				if (!IsLooping) continue;

				DataGot?.Invoke(this, new(await ReadByte()));
			}

			NS?.Close();
			TC?.Close();
		}

		List<byte> RBytesLRec = new List<byte>();
		async Task<string> Read() => Enc.GetString(await ReadByte()).Replace("\n", string.Empty);

		async Task<byte[]> ReadByte()
		{
			List<byte> RBytesL = RBytesLRec;
			try
			{
				while (NS?.DataAvailable == false && IsLooping)
					await Task.Delay(1);
			}
			catch (Exception e)
			{
				Log($"Error has occured at data waiting process\n{e}");
			}

			if (!IsLooping)
				return Array.Empty<byte>();

			byte[] b = new byte[4096];
			int nsreadr = 0;

			while (NS?.DataAvailable == true && !RBytesL.Contains((byte)'\n'))
			{
				nsreadr = await NS.ReadAsync(b.AsMemory());
				if (nsreadr <= 0)
					break;

				RBytesL.AddRange(b[0..nsreadr]);
			}

			if (!RBytesL.Contains((byte)'\n'))
			{
				if (RBytesLRec.Count == 0)
					RBytesLRec = RBytesL;
				else
					RBytesLRec.InsertRange(RBytesLRec.Count - 1, RBytesL);
				return
					Array.Empty<byte>();
			}

			return RBytesL.ToArray();
		}

		public void OnBSMDChanged(in BIDSSharedMemoryData data) { }
		public void OnOpenDChanged(in OpenD data) { }
		public void OnPanelDChanged(in int[] data) { }
		public void OnSoundDChanged(in int[] data) { }

		public void Print(in string data)
		{
			if (TC?.Connected != true || NS?.CanWrite != true) return;
			if (IsDebug)
				Log($">> {data}");

			try
			{
				byte[] wbytes = Enc.GetBytes(data + (data.EndsWith("\n") ? string.Empty : "\n"));
				NS?.Write(wbytes, 0, wbytes.Length);
			}
			catch (Exception e)
			{
				Log($"In Writing Process, An Error has occured\n{e}");
			}
		}
		public void Print(in byte[] data)
		{
			if (TC?.Connected != true || NS?.CanWrite != true) return;

			if (data?.Length > 0) NS?.Write(data, 0, data.Length);
		}

		readonly string[] ArgInfo = new string[]
		{
			"Argument Format ... [Header(\"-\" or \"/\")][SettingName(B, P etc...)][Separater(\":\")][Setting(38400, 2 etc...)]",
			"  -E or -Encoding : Set the Encoding Option.  Default:0  If you want More info about this argument, please read the source code.",
			"  -N or -Name : Set the Instance Name.  Default:\"tcp\"  If you don't set this option, this program maybe cause some bugs.",
			"  -P or -PortName : Set PortNum.  Default:14147  Only Number is allowed in the setting.",
			"  -RTO or -ReadTimeout : Set the ReadTimeout Setting.  Default:10000",
			"  -WTO or -WriteTimeout : Set the WriteTimeout Setting.  Default:1000"
		};
		public void WriteHelp(in string args)
		{
			Console.WriteLine("BIDS Server Program TCP Interface");
			Console.WriteLine("Version : " + Version.ToString());
			Console.WriteLine("Copyright (C) Tetsu Otter 2019");
			foreach (string s in ArgInfo) Console.WriteLine(s);
		}

		private void Log(object obj, [CallerMemberName] string? memberName = null)
			=> Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffff}]({Name}.{memberName}): {obj}");
	}
}
