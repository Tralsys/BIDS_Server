using System;
using System.IO;
using System.Linq;

namespace BIDS_Server;

partial class Program
{
	static void PrintSelectableModList()
	{
		try
		{
			string[] fs = LSMod();
			if (fs?.Length > 0)
			{
				foreach (var v in fs)
				{
					string[] cn = v.Split('.');
					Console.WriteLine($" {cn[^2]} : {v}");
				}
			}
			else
			{
				Console.WriteLine($"There are no modules in the mods folder. ({targetDirectory.FullName})");
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}

	static DirectoryInfo targetDirectory => new(Path.Combine(Directory.GetCurrentDirectory(), "mods"));
	static string[] LSMod()
	{
		if (!targetDirectory.Exists)
		{
			targetDirectory.Create();
			Console.WriteLine($"Created \"mods\" folder. ({targetDirectory.FullName})");
		}

		FileInfo[] fileInfoArr = targetDirectory.GetFiles("*.dll");

		return fileInfoArr.Select(v => v.Name).ToArray();
	}
}
