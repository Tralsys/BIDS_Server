using System;

namespace TR.BIDSsv
{
	public class BlankMOD : TR.IBIDSsv
	{
		public bool IsDisposed { get; private set; }
		public int Version { get; set; }
		public string Name { get; private set; }
		public bool IsDebug { get; set; }

		public bool Connect(in string args)
		{
			Name = "BlankMOD" + DateTime.UtcNow.ToString("hhmmssff");
			Console.WriteLine("BlankMOD Name:{0}", Name);
			return true;
		}

		public void Dispose() { IsDisposed = true; }

		public void OnBSMDChanged(in BIDSSharedMemoryData data) { }

		public void OnOpenDChanged(in OpenD data) { }

		public void OnPanelDChanged(in int[] data) { }

		public void OnSoundDChanged(in int[] data) { }

		public void Print(in string data) { }

		public void Print(in byte[] data) { }

		public void WriteHelp(in string args) { }
	}
}
