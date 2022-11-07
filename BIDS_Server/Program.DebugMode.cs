using System;
namespace BIDS_Server;

partial class Program
{
	void DebugMode(string targetInstanceName)
	{
		if (SvCore.ServerParserDic.Count <= 0)
		{
			Console.WriteLine("DebugDo : There are no connection.");
			return;
		}

		Console.WriteLine("Debug Start");

		SvCore.SetDebugMode(targetInstanceName, true);

		_ = Console.ReadKey(true);

		SvCore.SetDebugMode(targetInstanceName, false);

		Console.WriteLine("Debug End");
	}
}
