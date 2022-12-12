using System;
using System.Collections.Generic;
using System.Linq;

using BIDS.Parser;
using BIDS.Parser.Variable;

using TR.BIDSSMemLib;

namespace TR;

public partial class BIDSServerCore
{
	static BIDSServerCore? _Default = null;
	static public BIDSServerCore Default
	{
		get
		{
			_Default ??= new();
			return _Default;
		}
	}

	public readonly int Version = 202;
	private bool disposedValue;

	public SMemLib SMem { get; }

	public IReadOnlyDictionary<IBIDSsv, IParser> ServerParserDic => _ServerParserDic;
	Dictionary<IBIDSsv, IParser> _ServerParserDic { get; } = new();

	public BIDSServerCore(int Interval = 2, bool isNoSMemMode = false, bool isNoEventMode = false)
	{
		if (Interval <= 0)
			throw new ArgumentException("must not be 0 or less", nameof(Interval));

		SMem = new(isNoSMemMode, isNoEventMode);

		SMem.ReadStart(SMemLib.ARNum.All, Interval);

		SMem.SMC_BSMDChanged += SMem_SMC_BSMDChanged;
		SMem.SMC_PanelDChanged += SMem_SMC_PanelDChanged;
		SMem.SMC_SoundDChanged += SMem_SMC_SoundDChanged;

		// Variable SMem Reader
		Reader = new(Interval);
		Reader.NameAdded += Reader_NameAdded;
		Reader.ValueChanged += Reader_ValueChanged;
		Reader.Run();
	}

	public bool AddMod(in IBIDSsv sv)
	{
		Parser parser = new();
		bool isSuccess = _ServerParserDic.TryAdd(sv, parser);

		if (isSuccess)
		{
			sv.DataGot += Mod_DataGot;
			sv.Disposed += Mod_Disposed;

			if (sv is IBIDSsv.IManager manager)
			{
				manager.AddMod += Manager_AddMod;
				manager.RemoveMod += Manager_RemoveMod;
			}
		}

		return isSuccess;
	}

	private void Manager_RemoveMod(object? sender, ControlModEventArgs e)
	{
		RemoveMod(e.Instance);
	}

	private void Manager_AddMod(object? sender, ControlModEventArgs e)
	{
		AddMod(e.Instance);
	}

	private void Mod_Disposed(object? sender, EventArgs e)
	{
		if (sender is IBIDSsv sv)
			RemoveMod(sv);
	}

	public bool RemoveMod(string instanceName)
	{
		if (string.IsNullOrEmpty(instanceName))
			return false;

		foreach (var v in _ServerParserDic.Keys.Where(v => v.Name == instanceName).ToArray())
			_ = RemoveMod(v);

		return true;
	}

	public bool RemoveMod(in IBIDSsv sv)
	{
		// イベントのunsubscribeは、subscribeしていなくても失敗しない。
		sv.Disposed -= Mod_Disposed;
		sv.DataGot -= Mod_DataGot;

		if (sv is IBIDSsv.IManager manager)
		{
			manager.AddMod -= Manager_AddMod;
			manager.RemoveMod -= Manager_RemoveMod;
		}

		// 正常に実装されていれば、MOD側で2回Disposeされることは無い。
		sv.Dispose();

		return _ServerParserDic.Remove(sv, out IParser? parser);
	}

	public void ClearMod()
	{
		foreach (var v in ServerParserDic.Keys)
			v.Dispose();

		_ServerParserDic.Clear();
	}

	public void SetDebugMode(in string instanceName, in bool isDebug)
	{
		bool isEmptyName = string.IsNullOrEmpty(instanceName);

		foreach (var v in ServerParserDic.Keys)
			if (isEmptyName || v.Name == instanceName)
				v.IsDebug = isDebug;
	}

	public bool PrintCommand(string Name, string Command)
	{
		if (string.IsNullOrEmpty(Command) || ServerParserDic.Count <= 0)
			return false;

		foreach (var v in ServerParserDic.Keys)
		{
			if (v.Name != Name)
				continue;

			try
			{
				v.Print(Command);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
		}

		return true;
	}
}
