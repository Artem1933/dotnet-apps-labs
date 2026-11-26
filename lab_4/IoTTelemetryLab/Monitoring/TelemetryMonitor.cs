using IoTTelemetryLab.Evaluation;
using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Monitoring;

public class TelemetryMonitor
{
    private readonly SensorEvaluatorRegistry _registry;
    private readonly SensorType _sensorType;

    public event Action<TelemetryReading, SafetyStatus>? OnStatusEvaluated;

    public TelemetryMonitor(
        SensorEvaluatorRegistry registry,
        SensorType sensorType)
    {
        _registry = registry;
        _sensorType = sensorType;
    }

    public void Process(TelemetryReading reading)
    {
        var status = _registry.Evaluate(
            _sensorType,
            reading.Value);

        OnStatusEvaluated?.Invoke(reading, status);
    }
}