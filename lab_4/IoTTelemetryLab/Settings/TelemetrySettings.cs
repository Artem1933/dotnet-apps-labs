// Settings/TelemetrySettings.cs
namespace IoTTelemetryLab.Settings;

public class TelemetrySettings
{
    public int ReadingCount { get; set; } = 50;
    public double MinValue { get; set; } = -10;
    public double MaxValue { get; set; } = 100;
}
