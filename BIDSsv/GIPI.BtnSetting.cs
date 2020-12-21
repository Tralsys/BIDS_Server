using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TR.BIDSsv
{
	/// <summary>ボタンアサイン</summary>
	public struct BtnAssign
	{
		/// <summary>ボタン番号 (コマンドで送られてくる番号)</summary>
		public uint Num;
		/// <summary>ボタンの役割 (BVEに送出する番号)</summary>
		public int Job;
	}
	/// <summary>Buttonについて, GIPI互換となるよう, Assign変更機能を提供します.</summary>
	static public class GIPI
	{
		/// <summary>Button Assign List</summary>
		static public List<BtnAssign> BtnAssignList { get; private set; } = new List<BtnAssign>();

		/// <summary>ファイル名を指定して, ファイルから設定を読み込みます</summary>
		/// <param name="FName">設定ファイル名</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public bool LoadFromFile(string FName) => LoadFromStream(new StreamReader(FName));
		/// <summary>Streamから設定を読み込みます.</summary>
		/// <param name="s">設定ファイルのStream</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public bool LoadFromStream(StreamReader s)
		{
			using(StreamReader f = s)
				return LoadFromSArr(f?.ReadToEnd().Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries), ",");
		}
		/// <summary>String Listから設定を読み込みます</summary>
		/// <param name="sa">String List</param>
		/// <param name="separator">設定同士を隔てるセパレーター</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public bool LoadFromSArr(List<string> sa, string separator) => LoadFromSArr(sa?.ToArray(), separator);
		/// <summary>String Arrayから設定を読み込みます</summary>
		/// <param name="sa">String Array</param>
		/// <param name="separator">設定同士を隔てるセパレーター</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public bool LoadFromSArr(string[] sa, string separator)
		{
			if (sa != null && sa.Length != 0 && string.IsNullOrEmpty(separator)) return false;

			List<BtnAssign> ba = new List<BtnAssign>();
			string s = string.Empty;
			string[] ssa;
			uint n;
			int j;
			for(int i = 0; i < sa.Length; i++)
			{
				ssa = sa[i].Split(new string[] { separator, " " }, StringSplitOptions.RemoveEmptyEntries);
				try
				{
					n = uint.Parse(ssa[0]);
					j = int.Parse(ssa[1]);
				}catch(Exception e)
				{
					Console.WriteLine(e);
					return false;
				}
				ba.Add(new BtnAssign() { Num = n, Job = j });
			}

			return true;
		}

		/// <summary>変換テーブルの初期化, あるいは要素の挿入を行う</summary>
		/// <param name="ba"></param>
		/// <param name="init"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public bool SetTable(List<BtnAssign> ba, bool init = true)
		{
			if (init) BtnAssignList.Clear();
							
			BtnAssignList.InsertRange(init ? 0 : BtnAssignList.Count, ba);
			return true;
		}
		/// <summary>変換テーブルの初期化, あるいは要素の挿入を行う</summary>
		/// <param name="ba"></param>
		/// <param name="init"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public bool SetTable(BtnAssign[] ba, bool init = true)
		{
			if (init) BtnAssignList.Clear();

			BtnAssignList.InsertRange(init ? 0 : BtnAssignList.Count, ba);
			return true;
		}
		/// <summary>Buttonの役割を取得</summary>
		/// <param name="Num">Button番号</param>
		/// <returns>Buttonの役割</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public int[] GetBtJobNum(int Num) => GetBtJobNum((uint)Num);
		/// <summary>Buttonの役割を取得</summary>
		/// <param name="Num">Button番号</param>
		/// <returns>Buttonの役割</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public int[] GetBtJobNum(uint Num)
		{
			if (!(BtnAssignList?.Count > 0)) return null;
			List<int> jbl = new List<int>();

			Parallel.For(0, BtnAssignList.Count, (i) =>
			{
				if (BtnAssignList[i].Num == Num) jbl.Add(BtnAssignList[i].Job);
			});

			return jbl.Count > 0 ? jbl.ToArray() : null;
		}
	}
}
