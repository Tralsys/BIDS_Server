using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using TR.BIDSSMemLib;

namespace TR.BIDSsv
{
	/// <summary>Communication.dllで使用されているデータ構造</summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct CommunicationStruct
	{
		/// <summary>Header</summary>
		public UInt32 Header;
		/// <summary>列車位置 [m]</summary>
		public double Location;
		/// <summary>列車速度 [km/h]</summary>
		public float speed;
		/// <summary>0時からの経過時間 [ms]</summary>
		public int time;
		/// <summary>BC Pressure [kPa]</summary>
		public float BCPres;
		/// <summary>MR Pressure [kPa]</summary>
		public float MRPres;
		/// <summary>ER Pressure [kPa]</summary>
		public float ERPres;
		/// <summary>BP Pressure [kPa]</summary>
		public float BPPres;
		/// <summary>SAP Pressure[kPa]</summary>
		public float SAPPres;
		/// <summary>電流 [A]</summary>
		public float Current;
		/// <summary>ドアが閉まっているかどうか</summary>
		public int isDoorClosed;
		/// <summary>速度の絶対値 [km/h]</summary>
		public float SpeedAbs;
		/// <summary>フレーム間経過時間 [ms]</summary>
		public int deltaT;
		/// <summary>制動ハンドルの位置</summary>
		public int BrakeHPos;
		/// <summary>力行ハンドルの位置</summary>
		public int PowerHPos;
		/// <summary>逆転ハンドルの位置</summary>
		public int ReverHPos;
		/// <summary>定速制御状態</summary>
		public int ConstSPD;
		/// <summary>制動ハンドルの設定可能位置数</summary>
		public int BrakeNNum;
		/// <summary>力行ハンドルの設定可能位置数</summary>
		public int PowerNNum;
		/// <summary>ATS確認を行う位置</summary>
		public int ATSCheckPos;
		/// <summary>67°位置</summary>
		public int B67NNum;
		/// <summary>編成両数</summary>
		public int CarCount;
		/// <summary>非常ブレーキ位置</summary>
		public int EBPos;
		/// <summary>常用最大段数?</summary>
		public int MaxServiceBrakeNNum;
	}

	static public class CommunicationDllConverter
	{
		/// <summary>Communication.dllで使用されているデータフォーマットのヘッダ</summary>
		public const UInt32 CommunicationStructHeader = 0xfefef0f0;
		/// <summary>CommunicationStructのデータサイズ</summary>
		static public readonly int ComStrSize = Marshal.SizeOf(new CommunicationStruct());
		/// <summary>送受信するPanel/Sound配列長</summary>
		public const int PSArrSize = 256 * sizeof(int);

		/// <summary>Communication.dll規格のbyte配列データを適切な形式に変換する.</summary>
		/// <param name="ba"></param>
		/// <param name="PA"></param>
		/// <param name="SA"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public CommunicationStruct CommunicationBAGot(in byte[] ba, out int[] PA, out int[] SA)
		{
			PA = new int[256];
			SA = new int[256];

			IntPtr ip = Marshal.AllocHGlobal(ComStrSize);
			Marshal.Copy(ba, 0, ip, ComStrSize);
			if (Marshal.PtrToStructure(ip, typeof(CommunicationStruct)) is not CommunicationStruct cs)
			{
				Marshal.FreeHGlobal(ip);
				return default;
			}
			Marshal.FreeHGlobal(ip);

			ip = Marshal.AllocHGlobal(PSArrSize);
			Marshal.Copy(ba, ComStrSize, ip, PSArrSize);
			Marshal.Copy(ip, PA, 0, 256);
			Marshal.FreeHGlobal(ip);
			ip = Marshal.AllocHGlobal(PSArrSize);
			Marshal.Copy(ba, ComStrSize + PSArrSize, ip, PSArrSize);
			Marshal.Copy(ip, SA, 0, 256);
			Marshal.FreeHGlobal(ip);

			return cs;
		}
		/// <summary>BIDS Data to Communication.dll data</summary>
		/// <param name="cs">CommunicationStruct Data</param>
		/// <param name="PD">Panel Data</param>
		/// <param name="SD">Sound Data</param>
		/// <returns>Communication.dll Byte Array</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public byte[] CommunicationBAGet(in CommunicationStruct cs, in int[] PD, in int[] SD)
		{
			int[] pda = new int[256];
			int[] sda = new int[256];
			Buffer.BlockCopy(PD, 0, pda, 0, Math.Min(pda.Length, PD.Length) * sizeof(int));
			Buffer.BlockCopy(SD, 0, sda, 0, Math.Min(sda.Length, SD.Length) * sizeof(int));

			byte[] ba = new byte[ComStrSize + (PSArrSize * 2)];

			IntPtr ip = Marshal.AllocHGlobal(ComStrSize);
			Marshal.StructureToPtr(cs, ip, false);
			Marshal.Copy(ip, ba, 0, ComStrSize);
			Marshal.FreeHGlobal(ip);

			ip = Marshal.AllocHGlobal(PSArrSize);
			Marshal.Copy(pda, 0, ip, 256);
			Marshal.Copy(ip, ba, ComStrSize, PSArrSize);
			Marshal.FreeHGlobal(ip);
			ip = Marshal.AllocHGlobal(PSArrSize);
			Marshal.Copy(sda, 0, ip, 256);
			Marshal.Copy(ip, ba, ComStrSize + PSArrSize, PSArrSize);
			Marshal.FreeHGlobal(ip);

			return ba;
		}

		/// <summary>CommunicationStructをBIDSSharedMemoryDataに変換する</summary>
		/// <param name="cs"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public BIDSSharedMemoryData ComStrtoBSMD(this in CommunicationStruct cs) => cs.ComStrtoBSMD(out _);
		/// <summary>CommunicationStructをBIDSSharedMemoryDataに変換する</summary>
		/// <param name="cs"></param>
		/// <param name="deltaT">フレーム間経過時間[ms]</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public BIDSSharedMemoryData ComStrtoBSMD(this in CommunicationStruct cs, out int deltaT)
		{
			deltaT = cs.deltaT;
			return new BIDSSharedMemoryData()
			{
				IsEnabled = true,
				VersionNum = 202,
				IsDoorClosed = cs.isDoorClosed == 1,
				HandleData = new Hand()
				{
					B = cs.BrakeHPos,
					P = cs.PowerHPos,
					R = cs.ReverHPos,
					C = cs.ConstSPD
				},
				SpecData = new Spec()
				{
					A = cs.ATSCheckPos,
					B = cs.BrakeNNum,
					C = cs.CarCount,
					J = cs.B67NNum,
					P = cs.PowerNNum
				},
				StateData = new State()
				{
					BC = cs.BCPres,
					BP = cs.BPPres,
					ER = cs.ERPres,
					I = cs.Current,
					MR = cs.MRPres,
					SAP = cs.SAPPres,
					T = cs.time,
					V = cs.speed,
					Z = cs.Location
				}
			};
		}
		/// <summary>BIDSSharedMemoryDataをCommunicationStructに変換する</summary>
		/// <param name="bsmd"></param>
		/// <param name="deltaT">フレーム間経過時間[ms]</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		static public CommunicationStruct BSMDtoComStr(this in BIDSSharedMemoryData bsmd, int deltaT) => new CommunicationStruct()
		{
			Header = CommunicationStructHeader,
			deltaT = deltaT,
			ATSCheckPos = bsmd.SpecData.A,
			B67NNum = bsmd.SpecData.J,
			BCPres = bsmd.StateData.BC,
			BPPres = bsmd.StateData.BP,
			BrakeHPos = bsmd.HandleData.B,
			BrakeNNum = bsmd.SpecData.B,
			CarCount = bsmd.SpecData.C,
			ConstSPD = bsmd.HandleData.C,
			Current = bsmd.StateData.I,
			EBPos = bsmd.SpecData.B,
			ERPres = bsmd.StateData.ER,
			isDoorClosed = bsmd.IsDoorClosed ? 1 : 0,
			Location = bsmd.StateData.Z,
			MaxServiceBrakeNNum = bsmd.SpecData.J,
			MRPres = bsmd.StateData.MR,
			PowerHPos = bsmd.HandleData.P,
			PowerNNum = bsmd.SpecData.P,
			ReverHPos = bsmd.HandleData.R,
			SAPPres = bsmd.StateData.SAP,
			speed = bsmd.StateData.V,
			SpeedAbs = Math.Abs(bsmd.StateData.V),
			time = bsmd.StateData.T
		};
	}
}
