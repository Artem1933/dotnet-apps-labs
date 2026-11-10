namespace IoTTelemetryLab.Models;
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TelemetryRecord> Records { get; set; } = new();
}
