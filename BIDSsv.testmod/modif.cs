using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TR;
using TR.BIDSSMemLib;
using TR.BIDSsv;

namespace BIDSsv.testmod
{
	public class modif : IBIDSsv
	{
		public event EventHandler<DataGotEventArgs>? DataGot;
		public event EventHandler? Disposed;

		public bool IsDisposed { get; private set; }
		public int Version { get; set; } = ModVersion;
		private const int ModVersion = 202;
		public string Name { get; private set; } = "BsvTest";

		private string LogFileName = string.Empty;
		SemaphoreSlim? swr_lock = new SemaphoreSlim(1);//ref : https://www.atmarkit.co.jp/ait/articles/1411/18/news135.html
		private StreamWriter? swr = null;
		public bool IsDebug { get; set; }

		const int DefaultInterval = 10;
    bool IsBinaryAllowed = false;
		bool IsLoading = false;
		bool NoLogMode = false;

		List<CMDSendTimer> CSTL = new List<CMDSendTimer>();

    public bool Connect(in string args)
    {
			if (IsLoading) return true;
			IsLoading = true;

			string[] sa = args.Replace(" ", string.Empty).Split(new string[1] { "/" }, StringSplitOptions.RemoveEmptyEntries);

			DateTime dt = DateTime.UtcNow;
			Name += (dt.Minute * 60 * 1000 * dt.Second * 1000 + dt.Millisecond).ToString();
			LogFileName = string.Format("BsvTest.{0}.log", dt.ToString("yyyyMMdd.HHmmss.ffff"));
			Console.WriteLine("BIDSsv Test Mod\tVersion : {0}\tName : {1}\tFName : {2}", ModVersion, Name, LogFileName);
			try
			{
				swr = new StreamWriter(LogFileName);
				swr.AutoFlush = true;
				AppendText(string.Format("BIDSsv Test Mod version:{0}\tname:{1}", ModVersion, Name));
			}
			catch(Exception e)
			{
				Console.WriteLine("{0}.Connect : {1}", Name, e);
				return false;
			}

      for (int i = 0; i < sa.Length; i++)
      {
        string[] saa = sa[i].Split(':');
        if (saa.Length > 0)
        {
          try
          {
            switch (saa[0])
            {
							case "ASALL":
								AppendText(string.Format("sa[{0}]:{1} => ASALL", i, sa[i]));
								AppendText("ASALL Start");
								#region AS追加処理
								for (int j = -3; j <= 13; j++)
									SendCMD2sv(UFunc.BIDSCMDMaker(ConstVals.CMD_AUTOSEND_ADD, ConstVals.DTYPE_ELAPD, j));
								for (int j = -1; j <= 4; j++)
									SendCMD2sv(UFunc.BIDSCMDMaker(ConstVals.CMD_AUTOSEND_ADD, ConstVals.DTYPE_CONSTD, j));
								for (int j = -1; j <= 4; j++)
									SendCMD2sv(UFunc.BIDSCMDMaker(ConstVals.CMD_AUTOSEND_ADD, ConstVals.DTYPE_HANDPOS, j));
								for (int j = 0; j <= 2; j++)
									SendCMD2sv(UFunc.BIDSCMDMaker(ConstVals.CMD_AUTOSEND_ADD, ConstVals.DTYPE_DOOR, j));
								Parallel.For(0, 256, (j) =>
								{
									SendCMD2sv(UFunc.BIDSCMDMaker(ConstVals.CMD_AUTOSEND_ADD, ConstVals.DTYPE_PANEL, j));
									SendCMD2sv(UFunc.BIDSCMDMaker(ConstVals.CMD_AUTOSEND_ADD, ConstVals.DTYPE_SOUND, j));
								});
								for (int j = 0; j < 8; j++)
								{
									SendCMD2sv(UFunc.BIDSCMDMaker(ConstVals.CMD_AUTOSEND_ADD, ConstVals.DTYPE_PANEL_ARR, j));
									SendCMD2sv(UFunc.BIDSCMDMaker(ConstVals.CMD_AUTOSEND_ADD, ConstVals.DTYPE_SOUND_ARR, j));
								}
								#endregion
								AppendText("ASALL Complete");
								break;
              case "ONCE":
								if (saa.Length < 2)
								{
									AppendText(string.Format("Too Less Args to do ONCE : sa[{0}]", i));
									Console.WriteLine("{0} : Too Less Args to do ONCE: sa[{0}]", i);
									break;
								}
								else
								{
									AppendText(string.Format("sa[{0}]:{1} => ONCE do \"{2}\"", i, sa[i], saa[1]));
									SendCMD2sv(saa[1]);
								}
                break;
							case "BAREC":
								AppendText(string.Format("sa[{0}]:{1} => BAREC", i, sa[i]));
								IsBinaryAllowed = true;
								break;
							case "NOLOG":
								NoLogMode = true;
								break;
							default:
								if (saa[0].StartsWith("L"))
								{
									AppendText(string.Format("sa[{0}]:{1} => L Command", i, sa[i]));
									int interval = 0;
									try
									{
										interval = UFunc.String2INT(saa[0].Substring(1)) ?? DefaultInterval;
									}catch(Exception e)
									{
										AppendText(string.Format("Exceotion throwed at Parsing Process : {0}", e));
										Console.WriteLine("Exceotion throwed at Parsing Process : {0}", e);
										break;
									}

									CMDSendTimer cst;
									try
									{
										cst = new CMDSendTimer(this, interval, saa[1]);
									}catch(Exception e)
									{
										AppendText(string.Format("Exceotion throwed at Parsing Process : {0}", e));
										Console.WriteLine("Exceotion throwed at Parsing Process : {0}", e);
										break;
									}
									CSTL.Add(cst);
								}
								break;
            }
          }
          catch (Exception e)
          {
            Console.WriteLine("arg({0}) Error : {1}", sa[i], e);
            continue;
          }
        }
      }
			return true;
    }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		private void AppendText(string s)
		{
			DateTime dt = DateTime.UtcNow;
			Task.Run(async () =>
			{
				if (NoLogMode || disposing || swr == null || swr_lock == null) return;
				if (string.IsNullOrWhiteSpace(s)) return;
				try
				{
					await swr_lock.WaitAsync();
					if (IsDisposed || disposedValue) return;
					await swr.WriteLineAsync(dt.ToString("HH:mm:ss.fffff,\t") + s);
				}
				catch (ObjectDisposedException)
				{
					Console.WriteLine("{0} : StreamWriter Already Closed ({1})", Name, s);
					return;
				}
				catch (Exception e)
				{
					Console.WriteLine("{0}.AppendText({1}) : {2}", Name, s, e);
					Dispose();
					Console.WriteLine("{0} : Close this instance.", Name);
					Common.Remove(this);
					return;
				}
				finally
				{
					swr_lock?.Release();
				}
			});
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public bool SendCMD2sv(string cmd)
		{
			if (string.IsNullOrWhiteSpace(cmd)) return false;
			try
			{
				AppendText("Send : "+ cmd);

				DataGot?.Invoke(this, new(Encoding.Default.GetBytes(cmd)));

				return true;
			}catch(Exception e)
			{
				Console.WriteLine("{0}.SendCMD2sv({1}) : {2}", Name, cmd, e);
				return false;
			}
		}

		#region On_Changed
		public void OnBSMDChanged(in BIDSSharedMemoryData data) { }
		public void OnOpenDChanged(in OpenD data) { }
		public void OnPanelDChanged(in int[] data) { }
		public void OnSoundDChanged(in int[] data) { }
		#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void Print(in string data) => AppendText("Got : " + data);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void Print(in byte[] data)
		{
			if (!IsBinaryAllowed) return;
			AppendText("GotB: " + BitConverter.ToString(data));
		}

		string[] helps = new string[]
		{
			"BIDSsv Test Mod",
			"Version : " + ModVersion.ToString(),
			"# 説明",
			"BIDSsvの機能をテストします.  仮想のコマンドをBIDSsvに対して送信し, svより情報を受け取ります.",
			"mod内のほぼすべての操作はログファイルに記録されます.",
			"modロード時に指定したコマンドを, 指定されたタイミングで, 連続して送信します.",
			"実行中に任意のコマンドを送信する機能は, 現状実装されていません.",
			"# オプション一覧",
			"-ASALL :\tmod作成時に利用できるAutoSend機能をすべて有効化します(但しPanel/Soundは0~256)  使用例:\"/ASALL\"",
			"-ONCE :\t起動時に一度のみ送信するコマンドを指定します.  使用例:\"/ONCE:TRAE0\"",
			"-Lnn :\tnnに指定した長さの間隔をあけて, 指定したコマンドを送信します.  nnには整数を指定し, 単位はミリ秒です.  使用例:\"/L10:TRIE0\"",
			"-BAREC :\tByte Arrayの受信も記録します.  使用例:\"/BAREC\"",
			"-NOLOG :\tログ出力を無効化します.  なお, このオプションを指定するよりも前のログは記録されますので, ご了承ください.  また, ファイルハンドルは終了まで保持されます.",
			"※備考 : 本MODでは, オプション指定を半角スラッシュ(/)で始めます.  ハイフン(-)は負の数を表す記号として解釈されてしまいますので, ご了承ください."
		};
		public void WriteHelp(in string args)
		{
			foreach (var s in helps)
				Console.WriteLine(s);
		}

		#region IDisposable Support
		private bool disposedValue = false;
		private bool disposing = false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		protected virtual async void Dispose(bool disposing)
		{
			disposing = true;
			if (!disposedValue)
			{
				if (disposing)
				{
					AppendText("Dispose Start");
					if (CSTL?.Count > 0)
						for (int i = 0; i < CSTL.Count; i++)
							CSTL[i]?.Dispose();
					try
					{
						if (swr_lock is not null)
							await swr_lock.WaitAsync();
						//await swr?.FlushAsync();
						swr?.Close();
						swr?.Dispose();
					}
					finally
					{
						swr_lock?.Release();
					}
				}
				swr = null;
				swr_lock?.Dispose();
				swr_lock = null;
				disposedValue = true;
				Disposed?.Invoke(this, new());
			}
			IsDisposed = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void Dispose() => Dispose(true);
		#endregion

	}

	internal class CMDSendTimer : IDisposable
	{
		TimeSpan IntervalTS = new TimeSpan(0, 0, 0, 0, 10);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		internal CMDSendTimer(modif mif, in int interval, string cmd)
		{
			if (mif == null) throw new ArgumentNullException();
			IntervalTS = interval <= 0 ? new TimeSpan(1 - interval) : new TimeSpan(0, 0, 0, 0, interval);

			bool OnCmpleted = false;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
			void LoopReader()
			{
				if (OnCmpleted || mif?.IsDisposed != false || disposedValue) return;
				Task.Delay(IntervalTS).GetAwaiter().OnCompleted(LoopReader);
				if (mif?.SendCMD2sv(cmd) != true) OnCmpleted = true;
			}

			try
			{
				LoopReader();
			}
			catch (Exception e)
			{
				Console.WriteLine("CMDSender(interval:{0}, cmd:{1}) init : {2}", interval, cmd, e);
				Dispose();
				return;
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				//if (disposing){}// TODO: マネージ状態を破棄します (マネージ オブジェクト)。
				
				disposedValue = true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void Dispose() => Dispose(true);
		
		#endregion
	}
}
