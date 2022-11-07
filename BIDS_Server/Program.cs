using System;
using System.Threading.Tasks;

using TR;

namespace BIDS_Server
{
	partial class Program : IDisposable
	{
		const string VerNumStr = "012";

		static void Main(string[] args)
		{
			Console.WriteLine("BIDS Server Application");
			Console.WriteLine("ver : " + VerNumStr);

			using Program program = new();

			for (int i = 0; i < args.Length; i++)
				Console.WriteLine($"args[{i}] :: {args[i]}");


			program.Run(args);

			Console.WriteLine("Please press any key to exit...");
			Console.ReadKey();
		}

		BIDSServerCore SvCore { get; }

		public Program()
		{
			SvCore = new(1);
		}

		async void Run(string[] args)
		{
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].StartsWith("/"))
					{
						//do some process
						continue;
					}
					if (args[i].StartsWith("-"))
					{
						//do some process
						continue;
					}

					await ReadLineDO(args[i]);
				}
			}

			while (await ReadLineDO())
				continue;
		}

		Task<bool> ReadLineDO() => ReadLineDO(Console.ReadLine());
		async Task<bool> ReadLineDO(string? s)
		{
			if (string.IsNullOrEmpty(s))
				return true;

			if (s.EndsWith(".bsvcmd"))
			{
				LoadScriptFile(s);
				return true;
			}

			if (s.EndsWith(".btsetting"))
			{
				LoadButtonSetting(s);
				return true;
			}

			string[] cmd = s.Split(' ');

			switch (cmd[0].ToLower())
			{
				case "man":
					ShowManual(cmd.Length >= 2 ? cmd[1] : null);
					break;

				case "ls":
					PrintRunningModList();
					break;

				case "lsmods":
					PrintSelectableModList();
					break;

				case "exit":
					return false;

				case "close":
					SvCore.RemoveMod(cmd.Length >= 2 ? cmd[1] : string.Empty);
					break;

				case "debug":
					DebugMode(cmd.Length >= 2 ? cmd[1] : string.Empty);
					break;

				case "print":
					PrintCommand(cmd[1..]);
					break;

				case "delay":
					await Task.Delay(int.Parse(cmd[1]));
					break;

				default:
					LoadMod(cmd[0], s);
					break;
			}

			return true;
		}

		public void Dispose()
		{
			SvCore.Dispose();
		}
	}
}