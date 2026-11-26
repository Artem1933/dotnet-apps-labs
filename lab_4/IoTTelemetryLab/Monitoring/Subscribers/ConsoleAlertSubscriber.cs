using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Monitoring.Subscribers;

public static class ConsoleAlertSubscriber
{
    public static void OnStatusEvaluated(
        TelemetryReading reading,
        SafetyStatus status)
    {
        if (status == SafetyStatus.Critical)
        {
            Console.WriteLine(
                $"[КРИТИЧНО] {reading.DeviceId} о " +
                $"{reading.Timestamp:HH:mm:ss}: {reading.Value}");
        }
    }
}