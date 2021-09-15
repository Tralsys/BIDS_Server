using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;

using Mackoy.Bvets;

namespace TR.BIDSsv
{
	public class bvets_id : IInputDevice
	{
		static string CurrentDllPath { get; } = Assembly.GetExecutingAssembly().Location;
		static string CurrentDllDirectory { get; } = Path.GetDirectoryName(CurrentDllPath);
		static string CurrentDllFileNameWithoutExtension { get; } = Path.GetFileNameWithoutExtension(CurrentDllPath);

		static bvets_id()
		{
			if (!System.Diagnostics.Debugger.IsAttached)
				System.Diagnostics.Debugger.Launch();

			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string dllName = args.Name;

#pragma warning disable SecurityIntelliSenseCS
			//まずは自身と同じディレクトリから検索する
			string path = Path.Combine(CurrentDllDirectory, dllName);
			if (File.Exists(path))
				return Assembly.LoadFrom(path);

			//次に, 同じディレクトリにある「自身の名前(拡張子を除いたもの)と同じディレクトリ」を検索する
			path = Path.Combine(CurrentDllDirectory, CurrentDllFileNameWithoutExtension, dllName);
			if (File.Exists(path))
				return Assembly.LoadFrom(path);

			//最後に, 自身と同じディレクトリの子ディレクトリをすべて探索する
			string[] directories = Directory.GetDirectories(CurrentDllDirectory, "*", SearchOption.AllDirectories);
			foreach(string s in directories)
			{
				path = Path.Combine(s, dllName);
				if (File.Exists(path))
					return Assembly.LoadFrom(path);
			}
#pragma warning restore

			//それでも見つからなければNULLを返す
			return null;
		}

#pragma warning disable CS0067
		public event InputEventHandler LeverMoved;
		public event InputEventHandler KeyDown;
		public event InputEventHandler KeyUp;
#pragma warning restore

		static string SettingsFileName { get; } = typeof(bvets_id).Assembly.GetName().Name + ".xml";
		static XmlSerializer Serializer { get; } = new XmlSerializer(typeof(List<PortNameAndBaudRate>));
		Dictionary<PortNameAndBaudRate, IBIDSsv> Connections { get; set; }
		string SettingsFilePath { get; set; }

		public void Configure(IWin32Window owner) => new SettingWindow(Connections).ShowDialog();

		public void Dispose()
		{
			List<PortNameAndBaudRate> list = new();

			foreach(var i in Connections)
			{
				if (i.Value.IsDisposed)
					continue;

				list.Add(i.Key);
				i.Value.Dispose();
			}

			using StreamWriter sw = File.CreateText(SettingsFileName);
			Serializer.Serialize(sw.BaseStream, list);
		}

		public void Load(string settingsPath)
		{
			List<PortNameAndBaudRate> portNameAndBaudRates = new();
#pragma warning disable SecurityIntelliSenseCS
			if (File.Exists(settingsPath))
			{
				SettingsFilePath = Path.Combine(Path.GetDirectoryName(settingsPath), SettingsFileName);
			}else if (Directory.Exists(settingsPath))
			{
				SettingsFilePath = Path.Combine(settingsPath, SettingsFileName);
			}
#pragma warning restore

			if (File.Exists(SettingsFilePath))
			{
				using StreamReader sr = new(SettingsFilePath);
				portNameAndBaudRates = Serializer.Deserialize(sr) as List<PortNameAndBaudRate>;
			}
			else
			{
				using StreamWriter sw = File.CreateText(SettingsFilePath);
				Serializer.Serialize(sw.BaseStream, portNameAndBaudRates);
			}

			foreach(PortNameAndBaudRate i in portNameAndBaudRates)
			{
				Serial serial = new();
				string portName = i.PortName.StartsWith("COM") ? i.PortName.Substring(3) : i.PortName;
				try
				{
					serial.Connect($"-P:{portName} -B:{i.BaudRate}");
				}catch(Exception ex)
				{
					_ = System.Windows.Forms.MessageBox.Show($"Cannot Open SerialPort (PortName:{portName}, BaudRate:{i.BaudRate})\n{ex}", "BIDS_Server on InputDeviceIF", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		public void SetAxisRanges(int[][] ranges) { }

		public void Tick() { }
	}

	public record PortNameAndBaudRate(string PortName, int BaudRate);
}
