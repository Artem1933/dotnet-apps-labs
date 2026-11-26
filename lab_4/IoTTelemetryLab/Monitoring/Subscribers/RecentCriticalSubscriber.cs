using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Monitoring.Subscribers;

public class RecentCriticalSubscriber
{
    private const int Capacity = 5;
    private readonly Queue<TelemetryReading> _readings = new();

    public IReadOnlyList<TelemetryReading> LastReadings =>
        _readings.ToArray();

    public void OnStatusEvaluated(
        TelemetryReading reading,
        SafetyStatus status)
    {
        if (status != SafetyStatus.Critical)
        {
            return;
        }

        _readings.Enqueue(reading);

        if (_readings.Count > Capacity)
        {
            _readings.Dequeue();
        }
    }
}