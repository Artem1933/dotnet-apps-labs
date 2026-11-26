using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class VoltageEvaluator : ISensorEvaluator
{
    public SafetyStatus Evaluate(double value) => value switch
    {
        < 200 or > 240 => SafetyStatus.Critical,
        < 210 or > 230 => SafetyStatus.Warning,
        _ => SafetyStatus.Normal
    };
}