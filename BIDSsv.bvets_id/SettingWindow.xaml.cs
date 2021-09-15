using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TR.BIDSsv
{
	/// <summary>
	/// Interaction logic for SettingWindow.xaml
	/// </summary>
	public partial class SettingWindow : Window
	{
		private Dictionary<PortNameAndBaudRate, IBIDSsv> Connections { get; }
		private ObservableCollection<PortNameAndBaudRate> AvailableConnections { get; }

		public SettingWindow(Dictionary<PortNameAndBaudRate, IBIDSsv> connections)
		{
			Connections = connections;

			InitializeComponent();

			AvailableConnections = new();

			foreach (PortNameAndBaudRate i in connections.Keys)
				AvailableConnections.Add(i);

			ModSelectComboBox.ItemsSource = AvailableConnections;
		}

		SerialSettingViewModel CreateSerialSettingViewModel(in PortNameAndBaudRate data = null)
		{
			string[] availablePorts = SerialPort.GetPortNames();

			//有効なポートがなければ, とりあえず0 <= i < 256でポート名を作る
			if (availablePorts.Length <= 0) {
				List<string> list = new();

				for (int i = 0; i < 256; i++)
					list.Add($"COM{i}");

				availablePorts.ToArray();
			}

			SerialSettingViewModel vm = new()
			{
				AvailablePorts = availablePorts.ToImmutableArray(),
				IsSerialConnected = false,
				ConnectedBIDSSMemLibVersion = BIDSSMemLib.SMemLib.ReadBSMD().VersionNum
			};

			vm.SelectedBaudRate = data?.BaudRate ?? vm.SelectableBaudRate[0];
			vm.SelectedPortName = data?.PortName ?? vm.AvailablePorts[0];

			return vm;
		}

		private void Close_Clicked(object sender, RoutedEventArgs e) => Close();
		
		private void Add_Clicked(object sender, RoutedEventArgs e)
		{
			ModSelectComboBox.SelectedItem = null;
			SerialSetting serialSetting = new(CreateSerialSettingViewModel());
			serialSetting.SetRequested += NewInstance_SetRequested;
			serialSetting.CancelRequested += (s, e) =>
			{
				SettingViewBorder.Child = null;
				ModSelectComboBox.SelectedItem = null;
			};
			serialSetting.ReloadRequested += (s, e) => ReloadRequested(e);
		}
		private void Remove_Clicked(object sender, RoutedEventArgs e)
		{
			//選択されたModが存在しないなら実行しない
			if (ModSelectComboBox.SelectedItem is null)
				return;

			//選択されているModの情報を取得する
			PortNameAndBaudRate item = ModSelectComboBox.SelectedItem as PortNameAndBaudRate;

			//既存のConnectionに選択されたModが存在するか確認
			if (Connections.TryGetValue(item, out IBIDSsv ser))
			{
				//存在したら, 接続を解放する
				ser?.Dispose();

				//接続を絶ったので, 既存のConnectionから取り除く
				Connections.Remove(item);
			}

			//Modの選択を解除する
			ModSelectComboBox.SelectedItem = null;

			//接続を切ったModの情報をリストから削除する
			AvailableConnections.Remove(item);
		}

		private void ReloadRequested(EventArgsWithSerialSettingViewModel e, PortNameAndBaudRate initialValue = null)
		{
			string[] availablePorts = SerialPort.GetPortNames();

			//有効なポートがなければ, とりあえず0 <= i < 256でポート名を作る
			if (availablePorts.Length <= 0)
			{
				List<string> list = new();

				for (int i = 0; i < 256; i++)
					list.Add($"COM{i}");

				availablePorts.ToArray();
			}

			e.Target.AvailablePorts = availablePorts.ToImmutableArray();
			e.Target.ConnectedBIDSSMemLibVersion = BIDSSMemLib.SMemLib.ReadBSMD().VersionNum;

			if(initialValue is not null)
			{
				if (Connections.TryGetValue(initialValue, out IBIDSsv ser) && ser is not null)
					e.Target.IsSerialConnected = (ser as Serial)?.IsOpen ?? false;

				e.Target.SelectedBaudRate = initialValue.BaudRate;
				e.Target.SelectedPortName = initialValue.PortName;
			}
		}

		private void NewInstance_SetRequested(object sender, EventArgsWithSerialSettingViewModel e)
		{
			PortNameAndBaudRate data = new(e.Target.SelectedPortName, e.Target.SelectedBaudRate);

			if (data.BaudRate <= 0)
			{
				MessageBox.Show("BaudRateに0もしくは負の数を指定することはできません", "BIDS_Server InputDeviceIF", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			//既存の接続があれば, 一旦それを切る
			KeyValuePair<PortNameAndBaudRate, IBIDSsv> existingConnection = Connections.FirstOrDefault(i => i.Key.PortName == data.PortName);
			if (existingConnection.Value != default)
			{
				existingConnection.Value.Dispose();

				//念のためDictionaryにnullを入れて参照できないようにする
				Connections[existingConnection.Key] = null;

				//あるかどうかはわからんけど, 念のため.  エラーも出ないし.
				AvailableConnections.Remove(existingConnection.Key);
			}

			try
			{
				//接続を試行する
				Serial serial = new Serial();
				serial.Connect($"-P:{data.PortName} -B:{data.BaudRate}");

				//親から指定されたDictionaryにインスタンスを書き込む
				Connections[data] = serial;

				//ComboBox用のリストに同一の情報がない場合に限り追加する
				if (!AvailableConnections.Any(i => i == data))
					AvailableConnections.Add(data);

				ModSelectComboBox.SelectedItem = null;
				//正常に終了したことを通知する
				SettingViewBorder.Child = new TextBlock()
				{
					Text = $"Sucessed Opening Port ({data})",
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top,
					Margin = new Thickness(8)
				};
			}
			catch (Exception ex)
			{
				MessageBox.Show($"接続に失敗しました.  再度設定を見直してください ({data})\n{ex}");
				return;
			}
		}

		private void ModSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems is null)
			{
				SettingViewBorder.Child = null;
				return;
			}

			SettingViewBorder.Child = new SerialSetting(CreateSerialSettingViewModel(e.AddedItems as PortNameAndBaudRate));
		}
	}
}
