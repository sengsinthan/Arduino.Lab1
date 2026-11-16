using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Lab1.Mvvm;
using Lab1.Services;

namespace Lab1.ViewModels;

/// <summary>
/// Holds the UI state (port list, log) and the commands the buttons bind to.
/// All serial work is delegated to an <see cref="IArduinoService"/>.
/// </summary>
public class MainViewModel : ObservableObject
{
    private readonly IArduinoService _arduino;
    private readonly Dispatcher _dispatcher;   // marshals service events to the UI

    // Parameterless ctor so XAML can new it up; real service by default.
    public MainViewModel() : this(new ArduinoService()) { }

    public MainViewModel(IArduinoService arduino)
    {
        _arduino = arduino;
        _dispatcher = Dispatcher.CurrentDispatcher;
        _arduino.DataReceived += OnDataReceived;

        RefreshCommand = new RelayCommand(RefreshPorts);
        ConnectCommand = new RelayCommand(ToggleConnection);
        PingCommand = new RelayCommand(Ping, () => _arduino.IsConnected);
        LedOnCommand = new RelayCommand(() => SendCommand("LED ON"), () => _arduino.IsConnected);
        LedOffCommand = new RelayCommand(() => SendCommand("LED OFF"), () => _arduino.IsConnected);

        RefreshPorts();
    }

    // ---- Bound data ------------------------------------------------------

    public ObservableCollection<SerialPortInfo> Ports { get; } = new();

    private string? _selectedPort;
    public string? SelectedPort
    {
        get => _selectedPort;
        set => Set(ref _selectedPort, value);
    }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (Set(ref _isConnected, value))
            {
                OnPropertyChanged(nameof(ConnectButtonText));
                PingCommand.RaiseCanExecuteChanged();
                LedOnCommand.RaiseCanExecuteChanged();
                LedOffCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ConnectButtonText => IsConnected ? "Disconnect" : "Connect";

    private string _log = "";
    public string Log
    {
        get => _log;
        private set => Set(ref _log, value);
    }

    // ---- Commands --------------------------------------------------------

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ConnectCommand { get; }
    public RelayCommand PingCommand { get; }
    public RelayCommand LedOnCommand { get; }
    public RelayCommand LedOffCommand { get; }

    // ---- Behaviour -------------------------------------------------------

    private void RefreshPorts()
    {
        Ports.Clear();
        foreach (SerialPortInfo port in _arduino.GetAvailablePorts())
            Ports.Add(port);

        SelectedPort = Ports.Count > 0 ? Ports[0].PortName : null;
    }

    private void ToggleConnection()
    {
        if (_arduino.IsConnected)
        {
            _arduino.Disconnect();
            IsConnected = false;
            Append("Disconnected.");
            return;
        }

        if (SelectedPort is null)
        {
            Append("Pick a port first (click Refresh if the list is empty).");
            return;
        }

        try
        {
            _arduino.Connect(SelectedPort);
            IsConnected = true;
            Append($"Connected to {SelectedPort} @ 9600.");
        }
        catch (Exception ex)
        {
            Append("Could not open port: " + ex.Message);
        }
    }

    private void Ping() => SendCommand("PING");   // firmware replies "PONG"

    private void SendCommand(string command)
    {
        try
        {
            _arduino.Send(command);
            Append("> " + command);
        }
        catch (Exception ex)
        {
            Append("Write failed: " + ex.Message);
        }
    }

    private void OnDataReceived(object? sender, string text)
    {
        // Service raises this on a background thread; hop to the UI thread.
        _dispatcher.Invoke(() => Append("< " + text.TrimEnd()));
    }

    /// <summary>Call when the window closes to release the COM port.</summary>
    public void Shutdown() => _arduino.Disconnect();

    private void Append(string line) => Log += line + Environment.NewLine;
}
