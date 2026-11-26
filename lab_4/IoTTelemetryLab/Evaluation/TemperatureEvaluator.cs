using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class TemperatureEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        < 0 or > 80 => SafetyStatus.Critical,
        >= 60 and <= 80 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}