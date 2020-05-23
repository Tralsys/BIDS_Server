using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
	static public partial class Common
	{
		/// <summary>BIDSsvの対応バージョン</summary>
		static public readonly int Version = 202;
		/// <summary>デフォルトのポート番号</summary>
		static public readonly int DefPNum = 14147;


		static public BIDSSharedMemoryData BSMD
		{
			get => SMemLib.BIDSSMemData;
			set => SMemLib.Write(in value);
		}
		static public OpenD OD
		{
			get => SMemLib.OpenData;
			set => SMemLib.Write(in value);
		}
		static public PanelD PD
		{
			get => new PanelD() { Panels = SMemLib.PanelA };
			set => SMemLib.Write(in value);
		}
		static public SoundD SD
		{
			get => new SoundD() { Sounds = SMemLib.SoundA };
			set => SMemLib.Write(in value);
		}

		static public Hands Ctrl_Hand
		{
			get => CtrlInput.GetHandD();
			set => CtrlInput.SetHandD(ref value);
		}
		static public bool[] Ctrl_Key
		{
			get => CtrlInput.GetIsKeyPushed() ?? new bool[128];
			set => CtrlInput.SetIsKeyPushed(in value);
		}

		static public int PowerNotchNum
		{
			get => Ctrl_Hand.P;
			set => CtrlInput.SetHandD(CtrlInput.HandType.Power, value);
		}
		static public int BrakeNotchNum
		{
			get => Ctrl_Hand.B;
			set => CtrlInput.SetHandD(CtrlInput.HandType.Brake, value);
		}
		static public int ReverserNum
		{
			get => Ctrl_Hand.R;
			set => CtrlInput.SetHandD(CtrlInput.HandType.Reverser, value);
		}

		static private List<IBIDSsv> svlist = new List<IBIDSsv>();

		static private bool IsStarted = false;
		static private bool IsDebug = false;

		static public void Start(int Interval = 2, bool NO_SMEM_MODE = false, bool NO_Event_Mode = false)
		{
			if (!IsStarted)
			{
				SMemLib.Begin(NO_SMEM_MODE, NO_Event_Mode);
				SMemLib.ReadStart(0, Interval);

				BSMDChanged += Common_BSMDChanged;
				OpenDChanged += Common_OpenDChanged;
				PanelDChanged += Common_PanelDChanged;
				SoundDChanged += Common_SoundDChanged;
				IsStarted = true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public void Add<T>(ref T container) where T : IBIDSsv => svlist.Add(container);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public void Remove() => Remove(string.Empty);
		static public void Remove(string Name)
		{
			if (!(svlist?.Count > 0)) return;//null or 要素なしは実行しない
			
			for(int i = svlist.Count - 1; i >= 0; i--)//尻尾から順に
			{
				if (string.IsNullOrWhiteSpace(Name) || Equals(svlist[i].Name, Name))
				{
					try
					{
						Remove(svlist[i]);
					}
					catch(Exception e)
					{
						Console.WriteLine(e);
					}
				}
			}
		}
		static public void Remove(in IBIDSsv sv)
		{
			if (sv == null || !(svlist?.Count > 0)) return;
			string name = sv.Name;
			bool successd = false;
			try
			{
				ASRemove(sv);
				if (!sv.IsDisposed) sv.Dispose();
				successd = svlist.Remove(sv);
			}catch(Exception e)
			{
				Console.WriteLine("BIDSsv.Common Remove() Name:{0}\tan exception has occured.\n{1}", name, e);
			}
			
			Console.WriteLine("BIDSsv.Common : {0} Remove {1}", sv.Name, successd ? "done." : "failed");
		}

		static private void ASRemove(IBIDSsv sv)
		{
			try
			{
				if (PDAutoList?.Count > 0) PDAutoList.Remove(sv);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			try
			{
				if (SDAutoList?.Count > 0) SDAutoList.Remove(sv);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			try
			{
				if (AutoNumL?.Count > 0) AutoNumL.Remove(sv);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public void DebugDo() => DebugDo(string.Empty);
		static public void DebugDo(in string Name)
		{
			if (svlist.Count > 0)
			{
				Console.WriteLine("Debug Start");
				for (int i = 0; i < svlist.Count; i++)
					if (Name == string.Empty || Name == svlist[i].Name) svlist[i].IsDebug = true;
				IsDebug = Name == string.Empty;
				Console.ReadKey();
				IsDebug = false;
				for (int i = 0; i < svlist.Count; i++) svlist[i].IsDebug = false;
				Console.WriteLine("Debug End");
			}
			else Console.WriteLine("DebugDo : There are no connection.");
		}

		static public bool Print(string Name, string Command)
		{
			if (Name != null && Name != string.Empty)
			{
				for (int i = svlist.Count - 1; i >= 0; i--)
					if (Name == svlist[i].Name)
					{
						try
						{
							svlist[i].Print(Command);
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							return false;
						}
					}
			}
			else return false;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public string PrintList()
		{
			string s = string.Empty;
			if (svlist?.Count > 0) for (int i = 0; i < svlist.Count; i++) s += i.ToString() + " : " + svlist[i].Name + "\n";
			else s = "There are no connection.\n";
			return s;
		}

		static public void Dispose()
		{
			if (svlist.Count > 0) Parallel.For(0, svlist.Count, (i) => svlist[i].Dispose());
			SMemLib.ReadStop();
		}


		#region Obsolete Method
		[Obsolete("BIDSとして接尾辞等への配慮は行いません.  下位レイヤで独自に処理を行ってください.")]
		/// <summary>(disabled)Convert from Native Byte Array to BIDS Communication Byte Array Format</summary>
		/// <param name="ba">Array to Converted</param>
		/// <returns>Converted Array</returns>
		static public byte[] BAtoBIDSBA(in byte[] ba)
		{
			return ba;
			/*List<byte> dbl = ba.ToList();
			for(int i = 0; i < ba.Count(); i++)
			{
				switch (dbl[i])
				{
					case (byte)'\n':
						dbl.RemoveAt(i);
						byte[] r1 = new byte[2] { (byte)'\r', 0x01 };
						dbl.InsertRange(i, r1);
						i++;
						break;
					case (byte)'\r':
						i++;
						dbl.Insert(i, 0x02);
						break;
				}
			}
			dbl.Add((byte)'\n');
			return dbl.ToArray();*/
		}

		[Obsolete("BIDSとして接尾辞等への配慮は行いません.  下位レイヤで独自に処理を行ってください.")]
		/// <summary>(disabled)Convert from BIDS Communication Byte Array Format to Native Byte Array</summary>
		/// <param name="ba">Array to Converted</param>
		/// <returns>Converted Array</returns>
		static public byte[] BIDSBAtoBA(in byte[] ba)
		{
			return ba;
			/*if (ba.Length > 1)
			{
				List<byte> dbl = ba.ToList();
				//dbl.RemoveAt(dbl.Count - 1);
				for (int i = 0; i < dbl.Count - 1; i++)
				{
					if (dbl[i] == '\r')
					{
						switch (dbl[i + 1])
						{
							case 0x01:
								dbl[i] = (byte)'\n';
								dbl.RemoveAt(i + 1);
								break;
							case 0x02:
								dbl.RemoveAt(i + 1);
								break;
						}
					}
				}
				return dbl.ToArray();
			}
			else return ba;*/
		}
		#endregion
	}
}
