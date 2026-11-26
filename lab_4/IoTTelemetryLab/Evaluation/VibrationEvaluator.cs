using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class VibrationEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        > 5.0 => SafetyStatus.Critical,
        > 2.5 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}