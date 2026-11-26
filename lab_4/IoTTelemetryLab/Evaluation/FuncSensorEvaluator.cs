using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class FuncSensorEvaluator : ISensorEvaluator
{
    private readonly Func<double, SafetyStatus> _evaluate;

    public FuncSensorEvaluator(Func<double, SafetyStatus> evaluate)
    {
        _evaluate = evaluate;
    }

    public SafetyStatus Evaluate(double value) => _evaluate(value);
}