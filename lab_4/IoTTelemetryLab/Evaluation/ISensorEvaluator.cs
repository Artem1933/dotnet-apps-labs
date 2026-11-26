using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public interface ISensorEvaluator
{
    SafetyStatus Evaluate(double value);
}