using System;
using System.IO;
using TR.BIDSsv;

namespace BIDS_Server;

partial class Program
{
	static void LoadButtonSetting(string filePath)
	{
		Console.WriteLine($"{filePath} => Button assignment setting file to keep the compatibility with GIPI");

		using StreamReader sr = new(filePath);
		GIPI.LoadFromStream(sr);
	}
}
