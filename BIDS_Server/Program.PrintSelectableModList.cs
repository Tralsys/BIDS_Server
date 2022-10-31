using System;
using System.IO;

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
				int i = 0;
				foreach (var v in fs)
				{
					string[] cn = v.Split('.');
					Console.WriteLine($" {cn[^2]} : {v}");
				}
			}
			else
			{
				Console.WriteLine("There are no modules in the mods folder.");
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}

	static string[] LSMod()
	{
		string[] fl;
		try
		{
			fl = Directory.GetFiles(Path.Combine("mods", "*.dll"));

			if (fl.Length > 0)
			{
				for (int i = 0; i < fl.Length; i++)
					fl[i] = fl[i].Replace("mods" + Path.DirectorySeparatorChar, string.Empty);
			}

			return fl;
		}
		catch (DirectoryNotFoundException)
		{
			Directory.CreateDirectory(@"mods");
			Console.WriteLine("Created \"mods\" folder.");
		}

		return Array.Empty<string>();
	}
}
