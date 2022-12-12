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
				SMem.ReadStop(SMemLib.ARNum.All);
				Reader.Dispose();

				ClearMod();

				SMem.Dispose();
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
