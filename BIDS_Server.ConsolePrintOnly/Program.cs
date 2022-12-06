using System;
using TR;

namespace BIDS_Server.ConsolePrintOnly;

class Program
{
	static void Main(string[] args)
	{
		using BIDSServerCore sv = new(10);
		using ConsolePrintMod Mod01 = new(nameof(Mod01));
		using ConsolePrintMod Mod02 = new(nameof(Mod02));

		sv.AddMod(Mod01);
		sv.AddMod(Mod02);

		while (Console.ReadLine() is not (null or "exit" or "quit")) ;
	}
}

