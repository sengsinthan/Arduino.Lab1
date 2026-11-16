using System;
using System.Collections.Generic;

namespace Lab1.Services;

/// <summary>
/// Abstracts talking to the Arduino over a serial port, so the view model
/// never touches SerialPort directly (and can be given a fake in tests).
/// </summary>
public interface IArduinoService
{
    bool IsConnected { get; }

    /// <summary>The serial ports currently available, with friendly names.</summary>
    IReadOnlyList<SerialPortInfo> GetAvailablePorts();

    /// <summary>Open the given port. Throws if it cannot be opened.</summary>
    void Connect(string portName);

    void Disconnect();

    /// <summary>Send one newline-terminated command (e.g. "PING").</summary>
    void Send(string command);

    /// <summary>
    /// Raised when text arrives from the Arduino. Fires on a background
    /// thread, so subscribers must marshal to the UI thread themselves.
    /// </summary>
    event EventHandler<string>? DataReceived;
}
