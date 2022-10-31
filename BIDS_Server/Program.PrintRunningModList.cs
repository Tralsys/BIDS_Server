using System;

namespace BIDS_Server;

partial class Program
{
	void PrintRunningModList()
	{
		int i = 0;
		foreach (var v in SvCore.ServerParserDic.Keys)
			Console.WriteLine($"[{i}] : {v.Name} ({v})");
	}
}
