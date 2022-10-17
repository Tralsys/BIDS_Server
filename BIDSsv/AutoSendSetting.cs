using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TR.BIDSSMemLib;

namespace TR.BIDSsv;

/// <summary>AutoSendに関する各種定数や設定など</summary>
public class AutoSendSetting
{
	/// <summary>BasicConstデータ群のヘッダ</summary>
	public const uint HeaderBasicConst = 0x74726201;
	/// <summary>BasicConstデータ群のデータコマンド長</summary>
	public const uint BasicConstSize = 20;
	/// <summary>BasicCommonデータ群のヘッダ</summary>
	public const uint HeaderBasicCommon = 0x74726202;
	/// <summary>BasicCommonデータ群のデータコマンド長</summary>
	public const uint BasicCommonSize = 53;
	/// <summary>BasicBVE5</summary>
	public const uint HeaderBasicBVE5 = 0x74726203;
	/// <summary>BasicOBVE</summary>
	public const uint HeaderBasicOBVE = 0x74726204;
	/// <summary>BasicHandle</summary>
	public const uint HeaderBasicHandle = 0x74726205;
	/// <summary>BasicHandle</summary>
	public const uint BasicHandleSize = 20;
	/// <summary>PanelArr</summary>
	public const uint HeaderPanel = 0x74727000;
	/// <summary>PanelArr</summary>
	public static readonly uint PanelSize = (uint)(ConstVals.PANEL_BIN_ARR_PRINT_COUNT + 1) * sizeof(int);
	/// <summary>SoundArr</summary>
	public const uint HeaderSound = 0x74727300;
	/// <summary>SoundArr</summary>
	public static readonly uint SoundSize = (uint)(ConstVals.SOUND_BIN_ARR_PRINT_COUNT + 1) * sizeof(int);

	/// <summary>BasicConstデータ群のデータコマンドを構成します.</summary>
	/// <param name="s">Specデータ</param>
	/// <param name="o">OpenDデータ</param>
	/// <returns>BasicConstデータ群のデータコマンド</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
	static public byte[] BasicConst(in Spec s, in OpenD o, in int Version)
	{
		byte[] ba = new byte[BasicConstSize];
		int i = 0;
		ba.SetBIDSHeader(ConstVals.BIN_CMD_INFO_DATA, (byte)ConstVals.BIN_CMD_INFOD_TYPES.SPEC, ref i);
		ba.ValueSet2Arr((short)Version, ref i);
		ba.ValueSet2Arr((short)s.B, ref i);
		ba.ValueSet2Arr((short)s.P, ref i);
		ba.ValueSet2Arr((short)s.A, ref i);
		ba.ValueSet2Arr((short)s.J, ref i);
		ba.ValueSet2Arr((short)s.C, ref i);
		ba.ValueSet2Arr((short)o.SelfBCount, ref i);
		return ba;
	}
	/// <summary>BasicCommon</summary>
	/// <param name="s"></param>
	/// <param name="DoorState">ドアが閉扉状態か</param>
	/// <param name="SigNum">信号の現示番号</param>
	/// <returns>データコマンド</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
	static public byte[] BasicCommon(in State s, byte DoorState, int SigNum = 0)
	{
		byte[] ba = new byte[BasicCommonSize];
		int i = 0;
		ba.SetBIDSHeader(ConstVals.BIN_CMD_INFO_DATA, (byte)ConstVals.BIN_CMD_INFOD_TYPES.STATE, ref i);
		ba.ValueSet2Arr((double)s.Z, ref i);
		ba.ValueSet2Arr((float)s.V, ref i);
		ba.ValueSet2Arr((float)s.I, ref i);
		ba.ValueSet2Arr((float)0.0, ref i);
		ba.ValueSet2Arr((int)s.T, ref i);
		ba.ValueSet2Arr((float)s.BC, ref i);
		ba.ValueSet2Arr((float)s.MR, ref i);
		ba.ValueSet2Arr((float)s.ER, ref i);
		ba.ValueSet2Arr((float)s.BP, ref i);
		ba.ValueSet2Arr((float)s.SAP, ref i);
		ba.ValueSet2Arr((int)SigNum, ref i);
		ba[i] = DoorState;
		return ba;
	}
	/// <summary>BVE5 Data Group</summary>
	/// <param name="o"></param>
	/// <returns>Data Command</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
	static public byte[] BasicBVE5(object o)
	{
		return null;
	}
	/// <summary>openBVE</summary>
	/// <param name="o"></param>
	/// <returns>Data Command</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
	static public byte[] BasicOBVE(in OpenD o)
	{
		byte[] ba = new byte[Marshal.SizeOf(new OpenD()) + 4];
		int i = 0;
		ba.SetBIDSHeader(ConstVals.BIN_CMD_INFO_DATA, (byte)ConstVals.BIN_CMD_INFOD_TYPES.OPEND, ref i);

		IntPtr ip = new IntPtr();
		Marshal.StructureToPtr(o, ip, true);
		Marshal.Copy(ip, ba, i, ba.Length);
		return ba;
	}
	/// <summary>Handle State</summary>
	/// <param name="h"></param>
	/// <param name="SelfB"></param>
	/// <returns>Data Command</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
	static public byte[] BasicHandle(in Hand h, int SelfB)
	{
		byte[] ba = new byte[BasicHandleSize];
		int i = 0;
		ba.SetBIDSHeader(ConstVals.BIN_CMD_INFO_DATA, (byte)ConstVals.BIN_CMD_INFOD_TYPES.HANDLE, ref i);
		ba.ValueSet2Arr((int)h.P, ref i);
		ba.ValueSet2Arr((int)h.B, ref i);
		ba.ValueSet2Arr((int)h.R, ref i);
		ba.ValueSet2Arr((int)SelfB, ref i);
		return ba;
	}
	/// <summary>パネル</summary>
	/// <param name="a">Source Array</param>
	/// <param name="SttInd">Start Index Number</param>
	/// <returns>Result Array</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
	static public byte[] BasicPanel(int[] a, int SttInd)
	{
		byte[] ba = new byte[PanelSize];
		Buffer.BlockCopy(HeaderPanel.GetBytes(), 0, ba, 0, sizeof(uint));
		ba[3] = (byte)SttInd;

		Parallel.For(0, ConstVals.PANEL_BIN_ARR_PRINT_COUNT, (i) =>
		{
			if (a.Length <= (SttInd * ConstVals.PANEL_BIN_ARR_PRINT_COUNT) + i) return;
			Buffer.BlockCopy(a[i].GetBytes(), 0, ba, (i + 1) * sizeof(int), sizeof(int));
		});

		return ba;
	}
	/// <summary>Sound Arr Auto Sender Command</summary>
	/// <param name="a"></param>
	/// <param name="SttInd"></param>
	/// <returns>Data Command</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
	static public byte[] BasicSound(int[] a, int SttInd)
	{
		byte[] ba = new byte[SoundSize];

		_ = Parallel.For(0, ConstVals.SOUND_BIN_ARR_PRINT_COUNT, (i) =>
		{
			if (a.Length <= (SttInd * ConstVals.SOUND_BIN_ARR_PRINT_COUNT) + i) return;
			Buffer.BlockCopy(a[i].GetBytes(), 0, ba, (i + 1) * sizeof(int), sizeof(int));
		});

		Buffer.BlockCopy(HeaderSound.GetBytes(), 0, ba, 0, sizeof(uint));
		ba[3] = (byte)SttInd;
		return ba;
	}
}
