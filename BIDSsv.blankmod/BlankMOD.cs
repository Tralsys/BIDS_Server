using System;

namespace TR.BIDSsv
{
	public class BlankMOD : IBIDSsv_Blank
	{
		public override bool Connect(in string args)
		{
			Name = "BlankMOD" + DateTime.UtcNow.ToString("hhmmssff");
			Console.WriteLine("BlankMOD Name:{0}", Name);
			return true;
		}

		public override void WriteHelp(in string args) { }
	}
}
