using System;

namespace BIDS_Server;

partial class Program
{
	void PrintCommand(ReadOnlySpan<string> cmd)
	{
		if (cmd.Length <= 0)
			return;

		foreach (var v in cmd[1..])
		{
			if (string.IsNullOrEmpty(v))
				continue;

			if (SvCore.PrintCommand(cmd[0], v) != true)
				break;
		}
	}
}
