using System;
using System.Collections.Generic;
using System.Text;

using BIDS.Parser;
using BIDS.Parser.Variable;

using TR;

namespace BIDS_Server.ConsolePrintOnly;

partial class Program
{
	static void Main(string[] args)
	{
		using BIDSServerCore sv = new(10);
		using ConsolePrintMod Mod01 = new(nameof(Mod01));
		using ConsolePrintMod Mod02 = new(nameof(Mod02));
		RandomValueGenerator rand = new();
		List<VariableStructure> variableStructures = new();

		sv.AddMod(Mod01);
		sv.AddMod(Mod02);

		while (true)
		{
			string? s = Console.ReadLine();
			if (s is null or "exit" or "quit")
				return;

			if (s is "gen")
			{
				VariableStructure structure = rand.GetRandomStructure($"Sample{rand.GetInt32()}");

				Log(AppendSMemStructure(new("Creating:\n"), structure.Records));

				IBIDSBinaryData registerCmd = new VariableStructureRegister(structure);

				Mod01.DataGotEventInvoke(registerCmd.GetBytesWithHeader());
				variableStructures.Add(structure);
			}
			else if (s is "update")
			{
				if (variableStructures.Count <= 0)
				{
					Log("Cannot UPDATE: `variableStructures` is empty", ConsoleColor.Red);
					continue;
				}

				int index = rand.GetUInt16() % variableStructures.Count;
				VariableStructure structure = variableStructures[index];
				VariableStructurePayload payload = rand.GetRandomPayload(structure);
				Log(AppendSMemStructure(new("Updating:\n"), payload.Values));

				IBIDSBinaryData payloadCmd = new VariablePayload(payload, structure);
				Mod01.DataGotEventInvoke(payloadCmd.GetBytesWithHeader());
			}
			else if (s.StartsWith("0x"))
				Mod01.DataGotEventInvoke(Convert.FromHexString(s[2..]));
			else
				Mod01.DataGotEventInvoke(Encoding.UTF8.GetBytes(s));
		}
	}
}
