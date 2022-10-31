using System;
using System.IO;

namespace BIDS_Server;

partial class Program
{
	async void LoadScriptFile(string filePath)
	{
		Console.WriteLine($"{filePath} => BIDS_Server Command preset file");

		try
		{
			string[] lines = await File.ReadAllLinesAsync(filePath);

			if (lines.Length <= 0)
			{
				Console.WriteLine($"{filePath} : There is no command in the file.");
				return;
			}

			int i = 0;
			foreach (var v in lines)
			{
				Console.WriteLine($"do {filePath}[{i++}] : `{v}`");
				_ = await ReadLineDO(v);
			}
		}
		catch (FileNotFoundException)
		{
			Console.WriteLine($"Program.LoadScriptFile : the file `{filePath}` is not found.");
		}
		catch (DirectoryNotFoundException)
		{
			Console.WriteLine($"Program.LoadScriptFile : the directory is not found. (file path:`{filePath}`)");
		}
		catch (Exception e)
		{
			Console.WriteLine($"Program.LoadScriptFile : an exception has occured (file path:`{filePath}`)\n{e}");
		}
	}
}
