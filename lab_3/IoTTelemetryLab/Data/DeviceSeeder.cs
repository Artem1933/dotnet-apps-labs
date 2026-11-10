using IoTTelemetryLab.Models;
using IoTTelemetryLab.Processing;

namespace IoTTelemetryLab.Data;
public static class DeviceSeeder
{
    private static readonly string[] Axes = { "X", "Y", "Z" };

    public static int SeedDevice(AppDbContext context, string deviceName,
        int readingCount = 50, double minValue = 0, double maxValue = 10)
    {
        deviceName = deviceName.Trim();
        if (deviceName.Length is < 1 or > 100)
            throw new ArgumentException("Назва має містити від 1 до 100 символів.");
        if (context.Devices.Any(d => d.Name == deviceName))
            throw new ArgumentException("Сенсор із такою назвою вже існує.");
        var device = new Device { Name = deviceName };
        var raw = TelemetryGenerator.GenerateRawBatch(deviceName, readingCount, minValue, maxValue);
        foreach (var reading in TelemetryCleaner.RemoveInvalid(raw))
            device.Records.Add(new TelemetryRecord
            {
                Value = reading.Value,
                Timestamp = reading.Timestamp,
                Axis = Axes[Random.Shared.Next(Axes.Length)]
            });
        context.Devices.Add(device);
        context.SaveChanges();
        return device.Records.Count;
    }
}
