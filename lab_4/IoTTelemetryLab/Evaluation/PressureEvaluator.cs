using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class PressureEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        < 95 or > 110 => SafetyStatus.Critical,
        < 98 or > 105 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}