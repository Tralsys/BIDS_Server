using System;

using TR.BIDSSMemLib;

namespace TR;

public partial class BIDSServerCore : IDisposable
{
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				ClearMod();

				SMem.ReadStop(SMemLib.ARNum.All);
				Reader.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
