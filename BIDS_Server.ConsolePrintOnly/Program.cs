using System;
using System.Text;
using TR;

namespace BIDS_Server.ConsolePrintOnly;

partial class Program
{
	static void Main(string[] args)
	{
		using BIDSServerCore sv = new(10);
		using ConsolePrintMod Mod01 = new(nameof(Mod01));
		using ConsolePrintMod Mod02 = new(nameof(Mod02));

		sv.AddMod(Mod01);
		sv.AddMod(Mod02);

		while (true)
		{
			string? s = Console.ReadLine();
			if (s is null or "exit" or "quit")
				return;

			if (s.StartsWith("0x"))
				Mod01.DataGotEventInvoke(Convert.FromHexString(s[2..]));
			else
				Mod01.DataGotEventInvoke(Encoding.UTF8.GetBytes(s));
		}
	}
}

