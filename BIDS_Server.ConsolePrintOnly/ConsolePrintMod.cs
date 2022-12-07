using System;
using System.Runtime.CompilerServices;

using TR;
using TR.BIDSSMemLib;

namespace BIDS_Server.ConsolePrintOnly;

public class ConsolePrintMod : IBIDSsv
{
	public bool IsDisposed { get; private set; }

	public int Version { get; set; }

	public string Name { get; }

	public bool IsDebug { get; set; }

	public event EventHandler<DataGotEventArgs>? DataGot;
	public event EventHandler? Disposed;

	public ConsolePrintMod(string name)
	{
		Name = name;
	}

	public bool Connect(in string args)
		=> true;

	public void Dispose()
	{
		IsDisposed = true;
		GC.SuppressFinalize(this);
	}

	public void OnBSMDChanged(in BIDSSharedMemoryData data)
		=> Log(data);

	public void OnOpenDChanged(in OpenD data)
		=> Log(data);

	public void OnPanelDChanged(in int[] data)
		=> Log(data);

	public void OnSoundDChanged(in int[] data)
		=> Log(data);

	public void Print(in string data)
		=> Log(data, ConsoleColor.Magenta);

	public void Print(in byte[] data)
		=> Log(
			Convert.ToHexString(data),
			BitConverter.ToInt32(data.AsSpan()[4..8]) == 0
				// Register
				? ConsoleColor.Green
				// Data Update
				: ConsoleColor.Blue
		);

	public void DataGotEventInvoke(byte[] bytes)
	{
		Log(Convert.ToHexString(bytes), ConsoleColor.Gray);
		DataGot?.Invoke(this, new(bytes));
	}

	public void WriteHelp(in string args)
	{
	}

	void Log(
		object obj,
		[CallerMemberName]
		string callerMemberName = ""
	)
		=> Console.WriteLine($"- {Name}({callerMemberName})\t[{DateTime.Now:HH:mm:ss.fff}]:\t`{obj}`");

	void Log(
		object obj,
		ConsoleColor textColor,
		[CallerMemberName]
		string callerMemberName = ""
	)
	{
		ConsoleColor lastColor = Console.ForegroundColor;
		Console.ForegroundColor = textColor;
		Log(obj, callerMemberName);
		Console.ForegroundColor = lastColor;
	}
}

