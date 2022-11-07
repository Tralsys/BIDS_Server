using System;
using TR;

namespace BIDS_Server;

partial class Program
{
	static void ShowManual(string? targetName)
	{
		if (string.IsNullOrEmpty(targetName))
		{
			Console.WriteLine("BIDS Server Application");
			Console.WriteLine("ver : " + VerNumStr + "\n");
			Console.WriteLine("auto : Set the Auto Sending Function");
			Console.WriteLine("close: Close the connection.");
			Console.WriteLine("delay : insert delay function.");
			Console.WriteLine("exit : close this program.");
			Console.WriteLine("ls : Print the list of the Name of alive connection.");
			Console.WriteLine("lsmods : Print the list of the Name of mods you can use.");
			Console.WriteLine("man : Print the mannual of command.");
			Console.WriteLine("  If you want to check the manual of each mod, please check the mod name and type \"man + (mod name)\"");
			return;
		}

		switch (targetName)
		{
			case "auto":
				Console.WriteLine("auto sending process turn on command : auto send will start when this command is entered.");
				Console.WriteLine("  Option : bve5, common, const, handle, obve, panel, sound");
				break;

			case "autodel":
				Console.WriteLine("auto sending process turn off command : auto send will stop when this command is entered.");
				Console.WriteLine("  Option : bve5, common, const, handle, obve, panel, sound");
				break;

			case "exit":
				Console.WriteLine("exit command : Used when user want to close this program.  This command has no arguments.");
				break;

			case "ls":
				Console.WriteLine("connection listing command : Print the list of the Name of alive connection.  This command has no arguments.");
				break;

			case "lsmods":
				Console.WriteLine("mods listing command : Print the list of the Name of mods you can use.  This command has no arguments.");
				break;

			case "close":
				Console.WriteLine("close connection command : Used when user want to close the connection.  User must set the Connection Name to be closed.");
				break;

			case "delay":
				Console.WriteLine("insert pause command : Insert Pause function.  You can set 0 ~ Int.Max[ms]");
				break;

			default:
				string modFilePath = FindMod(targetName);

				using (IBIDSsv? ibsv = LoadAndCreateModInstance(modFilePath))
				{
					if (ibsv is null)
					{
						Console.WriteLine("Mod not found.");
						break;
					}

					ibsv.WriteHelp(string.Empty);
					ibsv.Dispose();
				}
				break;
		}
	}
}
