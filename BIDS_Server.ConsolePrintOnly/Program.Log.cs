using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using BIDS.Parser.Variable;

using TR.BIDSSMemLib;

namespace BIDS_Server.ConsolePrintOnly;

partial class Program
{
	static StringBuilder AppendSMemInfoAndStructure(StringBuilder builder, int index, VariableSMem smem)
	=> AppendSMemInfoAndStructure(builder, index, smem.Structure.Name, smem.Members);

	static StringBuilder AppendSMemInfoAndStructure(StringBuilder builder, int index, string name, IEnumerable<VariableStructure.IDataRecord> list)
	{
		builder.AppendLine($"SMem[{index:D2}]: `{name}`");
		return AppendSMemStructure(builder, list);
	}

	static StringBuilder AppendSMemStructure(StringBuilder builder, IEnumerable<VariableStructure.IDataRecord> list)
		=> builder.AppendJoin('\n', list.Select((v, i) => $"\t[{i:D2}]: {v}"));

	static void Log(
		object obj,
		[CallerMemberName]
		string callerMemberName = ""
	)
		=> Console.WriteLine($"({callerMemberName})\t[{DateTime.Now:HH:mm:ss.fff}]:\t`{obj}`");

	static void Log(
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