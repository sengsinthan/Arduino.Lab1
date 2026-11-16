using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;

namespace Lab1.Services;

/// <summary>SerialPort-backed implementation of <see cref="IArduinoService"/>.</summary>
public class ArduinoService : IArduinoService
{
    // Must match BAUD_RATE in the Arduino firmware.
    private const int BaudRate = 9600;

    private SerialPort? _port;

    public bool IsConnected => _port is { IsOpen: true };

    public event EventHandler<string>? DataReceived;

    public IReadOnlyList<SerialPortInfo> GetAvailablePorts()
    {
        // Map "COM3" -> "Arduino Uno (COM3)" using WMI. If the lookup fails
        // for any reason, fall back to the bare port name.
        Dictionary<string, string> friendly = QueryFriendlyNames();

        return SerialPort.GetPortNames()
            .OrderBy(p => p)
            .Select(p => new SerialPortInfo(p, friendly.GetValueOrDefault(p, p)))
            .ToList();
    }

    // Reads device captions like "Arduino Uno (COM3)" from Win32_PnPEntity and
    // keys them by the "COMx" found in parentheses.
    private static Dictionary<string, string> QueryFriendlyNames()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name FROM Win32_PnPEntity WHERE Name LIKE '%(COM%)'");

            foreach (ManagementObject device in searcher.Get().Cast<ManagementObject>())
            {
                string? name = device["Name"]?.ToString();
                if (name is null) continue;

                int open = name.LastIndexOf('(');
                int close = name.LastIndexOf(')');
                if (open >= 0 && close > open)
                {
                    string port = name.Substring(open + 1, close - open - 1).Trim();
                    result[port] = name;
                }
            }
        }
        catch
        {
            // WMI unavailable — callers fall back to the bare port name.
        }
        return result;
    }

    public void Connect(string portName)
    {
        Disconnect();   // ensure any previous port is released first

        _port = new SerialPort(portName, BaudRate)
        {
            NewLine = "\n",
            ReadTimeout = 500
        };
        _port.DataReceived += OnSerialDataReceived;
        _port.Open();
    }

    public void Disconnect()
    {
        if (_port is null) return;

        _port.DataReceived -= OnSerialDataReceived;
        if (_port.IsOpen) _port.Close();
        _port.Dispose();
        _port = null;
    }

    public void Send(string command)
    {
        if (_port is not { IsOpen: true })
            throw new InvalidOperationException("Not connected.");

        _port.WriteLine(command);   // appends NewLine ("\n")
    }

    // Runs on a background thread; just read and re-raise. Marshaling to the
    // UI thread is the subscriber's responsibility.
    private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        string text = _port!.ReadExisting();
        DataReceived?.Invoke(this, text);
    }
}
