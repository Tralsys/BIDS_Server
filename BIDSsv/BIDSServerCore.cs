using System;
using System.Linq;
using System.Collections.Generic;
using TR.BIDSSMemLib;
using BIDS.Parser;
using System.Threading.Tasks;

namespace TR;

public partial class BIDSServerCore
{
	static public BIDSServerCore Default { get; }

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
	}

	public bool AddMod(in IBIDSsv sv)
		=> _ServerParserDic.TryAdd(sv, new Parser());

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
		sv.Dispose();

		return _ServerParserDic.Remove(sv);
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
}
