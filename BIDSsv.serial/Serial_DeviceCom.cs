using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
	/// <summary>
	/// デバイスとの通信を司る.
	/// </summary>
	internal class Serial_DeviceCom : IDisposable
	{
		/// <summary>Serial IFが疎通しているかどうか</summary>
		public bool IsOpen { get => serial?.IsOpen == true; }

		/// <summary>デバッグ用出力を有効にするかどうか</summary>
		public bool IsDebugging { get; set; }

		public event EventHandler<string> StringDataReceived;
		public event EventHandler<byte[]> BinaryDataReceived;

		private const string BINARY_DATA_HEADER = "B64E";
		private const string SERIAL_SETTING_HEADER = "S";
		


		string ReadBuf = string.Empty;
		/// <summary>使用するSerial IF</summary>
		SerialPort serial = null;

		/// <summary>インスタンスを初期化します.</summary>
		/// <param name="ser">使用するシリアルインターフェイス</param>
		public Serial_DeviceCom(in SerialPort ser)
		{
			if (ser == null) throw new ArgumentNullException();

			serial = ser;

			try
			{
				serial.Open();
			}catch(Exception e)
			{
				Console.WriteLine("Serial_DeviceCom intialize : Serial Port Open Error=>{0}", e);
				return;
			}

			serial.DataReceived += Serial_DataReceived;
		}

		private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			string gotData = string.Empty;
			SerialPort sp = (SerialPort)sender;
			gotData = sp.ReadExisting();
			if (string.IsNullOrWhiteSpace(gotData)) return;//要素なし

			if (IsDebugging) Console.WriteLine("Serial_DeviceCom.Serial_DataReceived() : DataGot=>{1}", gotData);

			string[] sa = (ReadBuf + gotData).Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			if (!(sa?.Length > 0)) return;//要素なし

			int sa_len = sa.Length;
			if (gotData.EndsWith("\n") || gotData.EndsWith("\r")) ReadBuf = string.Empty;
			else
			{
				//最後の要素が改行記号で終わらない場合, その要素は次に判定を行う.
				ReadBuf = sa.Last() ?? string.Empty;
				sa_len -= 1;
			}

			if (sa_len <= 0) return;//要素なし

			for(int i = 0; i < sa_len; i++)
			{
				if (string.IsNullOrWhiteSpace(sa[i])) continue;//空要素は無視

				if (sa[i].StartsWith(BINARY_DATA_HEADER))
					try
					{
						BinaryDataReceived?.Invoke(this, Convert.FromBase64String(sa[i].Substring(BINARY_DATA_HEADER.Length)));
					}
					catch (Exception ex)
					{
						Console.WriteLine("Serial_DataCom.Serial_DataReceived() BinaryDataReceieved Error : {0}", ex);
					}
				else if (sa[i].StartsWith(SERIAL_SETTING_HEADER)) Serial_Setting(sa[i]);
				else StringDataReceived?.Invoke(this, sa[i]);
			}

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
			if (!IsOpen) return false;//null or openしてないなら実行しない.
			if (string.IsNullOrWhiteSpace(s)) return false;
			try
			{
				if (IsDebugging) Console.WriteLine("Serial_DeviceCom.PrintString() : DataSend=>{0}", s);
				serial.WriteLine(s);
				return true;
			}catch(Exception e)
			{
				Console.WriteLine("Serial_DeviceCom.PrintString({0}) : {1}", s, e);
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
			}catch(Exception e)
			{
				Console.WriteLine("Serial_DeviceCom.PrintBinary() : {0}", e);
				return false;
			}
		}
		#endregion

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

				serial?.Dispose();

				disposedValue = true;
			}
		}

		// TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
		// ~Serial_DeviceCom()
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
}
