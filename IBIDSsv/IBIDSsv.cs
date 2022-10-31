using System;
using TR.BIDSSMemLib;

namespace TR
{
	public class HandleCtrlEvArgs : EventArgs
	{
		public int? Reverser;
		public int? Power;
		public int? Brake;
	}

	public class KeyCtrlEvArgs : EventArgs
	{
		public bool?[]? KeyState;
	}

	public class DataGotEventArgs : EventArgs
	{
		public byte[] Bytes { get; }

		public DataGotEventArgs(byte[] bytes)
		{
			Bytes = bytes;
		}
	}

	public class ControlModEventArgs : EventArgs
	{
		public IBIDSsv Instance { get; }

		public ControlModEventArgs(IBIDSsv sv)
		{
			Instance = sv;
		}
	}

	public interface IBIDSsv : IDisposable
	{
		event EventHandler<DataGotEventArgs>? DataGot;

		event EventHandler? Disposed;

		bool IsDisposed { get; }
		int Version { get; set; }
		string Name { get; }
		bool IsDebug { get; set; }
		bool Connect(in string args);
		void Print(in string data);
		void Print(in byte[] data);
		void OnBSMDChanged(in BIDSSharedMemoryData data);
		void OnOpenDChanged(in OpenD data);
		void OnPanelDChanged(in int[] data);
		void OnSoundDChanged(in int[] data);

		void WriteHelp(in string args);

		public interface IManager : IBIDSsv
		{
			event EventHandler<ControlModEventArgs>? AddMod;
			event EventHandler<ControlModEventArgs>? RemoveMod;
		}
	}
}
