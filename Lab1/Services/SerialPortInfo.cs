namespace Lab1.Services;

/// <summary>A serial port plus the friendly device name Windows reports for it.</summary>
public record SerialPortInfo(string PortName, string FriendlyName)
{
    // e.g. "Arduino Uno (COM3)". Falls back to just the port name.
    public string Display =>
        string.IsNullOrWhiteSpace(FriendlyName) ? PortName : FriendlyName;
}
