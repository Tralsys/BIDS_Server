using System;
using System.IO;
using System.Reflection;

using TR;

namespace BIDS_Server;

partial class Program
{
	void LoadMod(string cmd, string rawCommandStr)
	{
		string modName = FindMod(cmd);

		if (string.IsNullOrEmpty(modName))
		{
			Console.WriteLine($"Command({cmd}) Not Found");
			return;
		}


		if (LoadAndCreateModInstance(modName) is not IBIDSsv ibsv)
		{
			Console.WriteLine("The specified dll file does not implement the IBIDSsv interface.");
			return;
		}

		try
		{
			if (ibsv.Connect(rawCommandStr))
				SvCore.AddMod(ibsv);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			ibsv.Dispose();
			SvCore.RemoveMod(ibsv);
		}
	}

	static string FindMod(string keyword)
	{
		string[] modList = LSMod();

		if (modList.Length <= 0)
			return string.Empty;

		for (int i = 0; i < modList.Length; i++)
		{
			string[] sa = modList[i].Split('.');

			if (sa.Length >= 2 && sa[^2] == keyword)
				return modList[i];
		}

		return string.Empty;
	}

	//https://qiita.com/rita0222/items/609583c31cb7f0132086
	static IBIDSsv? LoadAndCreateModInstance(string fname)
	{
		Assembly? a;
		try
		{
			string filePath = Path.Combine(targetDirectory.FullName, fname);
			a = Assembly.LoadFrom(filePath);
		}
		catch (FileNotFoundException)
		{
			return null;
		}
		catch (Exception)
		{
			throw;
		}

		if (a is null)
			return null;

		try
		{
			foreach (var t in a.GetTypes())
			{
				if (t.IsInterface)
					continue;

				if (Activator.CreateInstance(t) is IBIDSsv ibs)
					return ibs;
			}
		}
		catch (ReflectionTypeLoadException e)
		{
			foreach (var ex in e.LoaderExceptions)
				Console.WriteLine(ex);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}

		return null;
	}
}
