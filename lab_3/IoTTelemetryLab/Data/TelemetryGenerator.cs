using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Data;

/// <summary>Генерує тестову телеметрію з імітацією апаратних помилок.</summary>
public static class TelemetryGenerator
{
    /// <summary>
    /// Створює задану кількість показів у діапазоні minValue–maxValue
    /// з інтервалом 5 секунд та ймовірністю апаратної помилки 10%.
    /// </summary>
    public static List<TelemetryReading> GenerateRawBatch(
        string deviceId,
        int count,
        double minValue,
        double maxValue)
    {
        var random = new Random();
        var readings = new List<TelemetryReading>();
        var baseTime = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            double value = random.NextDouble() < 0.1
                ? (random.Next(2) == 0 ? -999.0 : double.NaN)
                : Math.Round(
                    minValue + random.NextDouble() * (maxValue - minValue),
                    2);

            readings.Add(new TelemetryReading(
                DeviceId: deviceId,
                Value: value,
                Timestamp: baseTime.AddSeconds(i * 5)));
        }

        return readings;
    }
}