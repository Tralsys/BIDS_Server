using System;
using System.Text;

namespace TR
{
	public abstract class IBIDSsv2 : IDisposable
	{
		private bool disposedValue;
		protected bool DisposingValue { get; private set; } = false;

		public virtual bool IsDisposed { get; protected set; } = false;
		/// <summary>この通信インスタンスが使用するBIDS通信規約のバージョン</summary>
		public virtual int Version { get; protected set; } = 203;
		public virtual string Name { get; protected set; } = "IBIDSsv2_default_InstanceName";
		public bool IsDebug { get; set; } = false;
    public abstract bool Connect(in string args);
    public abstract void Print(in StringBuilder data);
    public abstract void Print(in byte[] data);
    public abstract void WriteHelp(in string args);

    public virtual void InstanceCMD(in string args) { }

		#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			DisposingValue = true;
			if (!disposedValue)
			{
				if (disposing) { }

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
