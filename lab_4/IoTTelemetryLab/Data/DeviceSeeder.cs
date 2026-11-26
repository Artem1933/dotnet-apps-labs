using IoTTelemetryLab.Models;
using IoTTelemetryLab.Processing;

namespace IoTTelemetryLab.Data;

public static class DeviceSeeder
{
    private static readonly string[] Axes = { "X", "Y", "Z" };

    public static async Task<int> SeedAsync(ITelemetryRepository repository, string deviceName,
        int readingCount, double minValue, double maxValue, CancellationToken ct = default)
    {
        var device = await repository.AddDeviceAsync(deviceName, ct);

        var raw = TelemetryGenerator.GenerateRawBatch(deviceName, readingCount, minValue, maxValue);
        var cleaned = TelemetryCleaner.RemoveInvalid(raw).ToList();

        var records = cleaned.Select(r => new TelemetryRecord
        {
            Value = r.Value,
            Timestamp = r.Timestamp,
            Axis = Axes[Random.Shared.Next(Axes.Length)]
        }).ToList();

        await repository.AddReadingsAsync(device.Id, records, ct);
        return records.Count;
    }
}
