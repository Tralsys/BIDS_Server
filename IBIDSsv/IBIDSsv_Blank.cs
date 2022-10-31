using System;
using System.Collections.Generic;
using System.Text;
using TR.BIDSSMemLib;

namespace TR
{
	public abstract class IBIDSsv_Blank : IBIDSsv
	{
		public event EventHandler<DataGotEventArgs>? DataGot;

		public bool IsDisposed { get; protected set; }
		public int Version { get; set; }
		public string Name { get; protected set; }
		public bool IsDebug { get; set; }

		public abstract bool Connect(in string args);

		public virtual void OnBSMDChanged(in BIDSSharedMemoryData data) { }

		public virtual void OnOpenDChanged(in OpenD data) { }

		public virtual void OnPanelDChanged(in int[] data) { }

		public virtual void OnSoundDChanged(in int[] data) { }

		public virtual void Print(in string data) { }

		public virtual void Print(in byte[] data) { }

		public abstract void WriteHelp(in string args);

		#region IDisposable Support
		protected bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			disposedValue = true;
		}

		public void Dispose() => Dispose(true);
		#endregion
	}
}
