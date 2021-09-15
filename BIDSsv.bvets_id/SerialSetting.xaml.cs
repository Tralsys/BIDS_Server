using System;
using System.Windows;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace TR.BIDSsv
{
	public partial class SerialSetting : UserControl
	{
		public event EventHandler<EventArgsWithSerialSettingViewModel> ReloadRequested;
		public event EventHandler<EventArgsWithSerialSettingViewModel> SetRequested;
		public event EventHandler<EventArgsWithSerialSettingViewModel> CancelRequested;

		private SerialSettingViewModel ViewModel { get; }

		public SerialSetting(SerialSettingViewModel vm)
		{
			ViewModel = vm;
			InitializeComponent();

			DataContext = vm;
		}

		private void Reload_Click(object sender, RoutedEventArgs e) => ReloadRequested?.Invoke(this, new(ViewModel));
		private void Cancel_Click(object sender, RoutedEventArgs e) => CancelRequested?.Invoke(this, new(ViewModel));
		private void Set_Click(object sender, RoutedEventArgs e) => SetRequested?.Invoke(this, new(ViewModel));
	}

	public class EventArgsWithSerialSettingViewModel : EventArgs
	{
		public EventArgsWithSerialSettingViewModel(SerialSettingViewModel arg) => Target = arg;
		public SerialSettingViewModel Target { get; }
	}

	public class SerialSettingViewModel : INotifyPropertyChanged, IReadOnlySerialSettingViewModel
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged<T>(ref T target, in T source, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
		{
			if (Equals(target, source))
				return;

			target = source;
			PropertyChanged?.Invoke(this, new(callerName));
		}

		private ImmutableArray<string> _AvailablePorts;
		public ImmutableArray<string> AvailablePorts
		{
			get => _AvailablePorts;
			set => OnPropertyChanged(ref _AvailablePorts, value);
		}

		private string _SelectedPortName = string.Empty;
		public string SelectedPortName { get => _SelectedPortName; set => OnPropertyChanged(ref _SelectedPortName, value); }

		public ImmutableArray<int> SelectableBaudRate { get; } = new int[]
		{
			110,
			300,
			1200,
			2400,
			4800,
			9600,
			19200,
			38400,
			57600,
			115200,
			230400
		}.ToImmutableArray();

		private int _SelectedBaudRate = 19200;
		public int SelectedBaudRate { get => _SelectedBaudRate; set => OnPropertyChanged(ref _SelectedBaudRate, value); }

		private int _ConnectedBIDSSMemLibVersion = 0;
		public int ConnectedBIDSSMemLibVersion { get => _ConnectedBIDSSMemLibVersion; set => OnPropertyChanged(ref _ConnectedBIDSSMemLibVersion, value); }

		private bool _IsSerialConnected = false;
		public bool IsSerialConnected { get => _IsSerialConnected; set => OnPropertyChanged(ref _IsSerialConnected, value); }
	}

	public interface IReadOnlySerialSettingViewModel
	{
		ImmutableArray<string> AvailablePorts { get; }
		string SelectedPortName { get; }
		int SelectedBaudRate { get; }
		int ConnectedBIDSSMemLibVersion { get; }
		bool IsSerialConnected { get; }
	}
}
