﻿using System;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
	/// <summary>
	/// デバイスとの通信を司る.
	/// </summary>
	internal class Serial_DeviceCom : IDisposable
	{
		#region 使用する変数類
		/// <summary>Serial IFが疎通しているかどうか</summary>
		public bool IsOpen { get => serial?.IsOpen == true; }

		/// <summary>デバッグ用出力を有効にするかどうか</summary>
		public bool IsDebugging { get; set; }

		public event EventHandler<string>? StringDataReceived;
		public event EventHandler<byte[]>? BinaryDataReceived;

		public Encoding Enc { get => serial?.Encoding ?? Encoding.ASCII; }

		public bool ReConnectWhenTimedOut { get; set; } = false;
		/// <summary>1秒の間隔をあけて, 生存報告をNULL文字送出にて行う.</summary>
		public bool IamAliveCMD { get; set; } = false;
		private readonly Task? AliveCMDTask = null;
		private readonly TimeSpan AliveCMDTimeSpan = new TimeSpan(0, 0, 1);
		private readonly TimeSpan ReConnectTimeSpan = new TimeSpan(0, 0, 0, 0, 100);

		private const string BINARY_DATA_HEADER = "B64E";
		private const string SERIAL_SETTING_HEADER = "S";
		readonly object Locker = new object();

		string ReadBuf = string.Empty;

		/// <summary>使用するSerial IF</summary>
		readonly SerialPort serial;
		#endregion

		/// <summary>インスタンスを初期化します.</summary>
		/// <param name="ser">使用するシリアルインターフェイス</param>
		public Serial_DeviceCom(in SerialPort ser)
		{
			if (ser is null)
				throw new ArgumentNullException(nameof(ser));

			serial = ser;

			try
			{
				lock (Locker)
				{
					serial.Open();
				}
			}
			catch (Exception e)
			{
				Log($"Serial Port Open Error=>{e}");
				return;
			}

			if (IamAliveCMD)
			{
				byte[] NULL_BA = new byte[1] { 0x00 };
				AliveCMDTask = new Task(async () =>
					{
						while (!disposingValue)
						{
							lock (Locker)
							{
								try
								{
									serial.Write(NULL_BA, 0, 1);
								}
								catch (TimeoutException e)
								{
									if (ReConnectWhenTimedOut)
									{
										ReConnect();
										continue;
									}
									else
									{
										Log(e, "AliveCmdCheck");
									}
								}
								catch (Exception e)
								{
									Log(e, "AliveCmdCheck");
									return;
								}
							}
							await Task.Delay(AliveCMDTimeSpan);
						}
					});//生存確認パケット送出用
			}

			serial.DataReceived += Serial_DataReceived;
		}


		private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			string gotData = string.Empty;
			SerialPort sp = (SerialPort)sender;
			try
			{
				if (!sp.IsOpen)
					Log("Serial Already Closed.");

				if (!sp.BaseStream.CanRead || !sp.BaseStream.CanWrite)
					Log($"Read or Write not allowed: CanRead:{sp.BaseStream.CanRead}, CanWrite:{sp.BaseStream.CanWrite}");

				lock (Locker)
				{
					try
					{
						gotData = sp.ReadExisting();
					}
					catch (InvalidOperationException)
					{
						if (ReConnectWhenTimedOut) ReConnect();
					}
				}
			}
			catch (Exception ex)
			{
				Log($"ReadExisting : {ex}");
			}

			if (string.IsNullOrWhiteSpace(gotData))
				return;//要素なし

			Task.Run(async () =>
			{

				if (IsDebugging)
					Log(gotData);

				string[] sa;
				try
				{
					sa = (ReadBuf + gotData).Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				}
				catch (Exception ex)
				{
					Log($"StringSplit : {ex}");
					return;
				}
				if (sa.Length <= 0)
					return;//要素なし

				int sa_len = sa.Length;
				if (gotData.EndsWith("\n") || gotData.EndsWith("\r")) ReadBuf = string.Empty;
				else
				{
					//最後の要素が改行記号で終わらない場合, その要素は次に判定を行う.
					ReadBuf = sa.Last() ?? string.Empty;
					sa_len -= 1;
				}

				if (sa_len <= 0) return;//要素なし

				for (int i = 0; i < sa_len; i++)
				{
					if (string.IsNullOrWhiteSpace(sa[i])) continue;//空要素は無視
					int ind = i;
					if (sa[ind].StartsWith(BINARY_DATA_HEADER))
						_ = Task.Run(() =>
							{
								try
								{
									BinaryDataReceived?.Invoke(this, Convert.FromBase64String(sa[ind].Substring(BINARY_DATA_HEADER.Length)));
								}
								catch (Exception ex)
								{
									Log($"BinaryDataReceieved Error : {ex}");
								}
							});
					else if (sa[i].StartsWith(SERIAL_SETTING_HEADER))
						await Task.Run(() => Serial_Setting(sa[ind]));
					else _ = Task.Run(() =>
					{
						try
						{
							StringDataReceived?.Invoke(this, sa[ind]);
						}
						catch (Exception ex)
						{
							Log($"StringDatReceived.Invoke Error : {ex}");
						}
					});
				}
			});
		}

		void Serial_Setting(string cmd)
		{

		}

		#region 出力メソッド
		/// <summary>文字列を出力</summary>
		/// <param name="s">出力する文字列</param>
		/// <returns>出力に成功したかどうか</returns>
		public bool PrintString(string s)
		{
			if (!IsOpen || string.IsNullOrWhiteSpace(s))
				return false;//null or openしてないなら実行しない.

			try
			{
				if (IsDebugging)
					Log(s);

				lock (Locker)
				{
					try
					{
						serial.WriteLine(s);
					}
					catch (TimeoutException)
					{
						Log($"\tTimeOut ({s})");
						if (ReConnectWhenTimedOut)
						{
							ReConnect();
							return PrintString(s);
						}
						return false;
					}
				}

				return true;
			}
			catch (Exception e)
			{
				Log($"Error with ({s}) : {e}");
				return false;
			}
		}

		/// <summary>Byte Arrayを適切な形で出力する.</summary>
		/// <param name="ba">出力するByte Array</param>
		/// <param name="offset">出力開始位置(nullで既定値"0")</param>
		/// <param name="length">出力するByte Arrayの長さ(nullで既定値"ba.length")</param>
		/// <returns>出力に成功したかどうか</returns>
		public bool PrintBinary(byte[] ba, int? offset = null, int? length = null)
		{
			if (!IsOpen) return false;//null or openしてないなら実行しない.
			if (!(ba?.Length > 0)) return false;//0以下の要素 or nullなら実行しない.
			try
			{
				return PrintString(BINARY_DATA_HEADER + Convert.ToBase64String(ba, offset ?? 0, length ?? ba.Length, Base64FormattingOptions.None));//PrintString側でデバッグ用出力を実施
			}
			catch (Exception e)
			{
				Log(e);
				return false;
			}
		}
		#endregion


		/// <summary>ロックは呼び出し元で取得してください.</summary>
		async void ReConnect()
		{
			Log("Reconnect doing...");
			try
			{
				if (serial?.IsOpen == true)
					serial.Close();
			}
			catch (Exception e)
			{
				Log($"SerialPort Close Failed. : {e}");
				return;
			}

			await Task.Delay(ReConnectTimeSpan);

			try
			{
				if (serial?.IsOpen == false)
					serial.Open();
			}
			catch (Exception e)
			{
				Log($"SerialPort ReOpen Failed. : {e}");
				return;
			}
		}

		private static void Log(object obj, [CallerMemberName] string? memberName = null)
			=> Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffff}]({nameof(Serial_DeviceCom)}.{memberName}): {obj}");

		#region IDisposable Support
		private bool disposedValue = false;
		private bool disposingValue = false;

		protected virtual void Dispose(bool disposing)
		{
			disposingValue = true;
			if (!disposedValue)
			{
				if (disposing)
				{
				}

				lock (Locker)
				{
					serial?.Dispose();
				}
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
}
