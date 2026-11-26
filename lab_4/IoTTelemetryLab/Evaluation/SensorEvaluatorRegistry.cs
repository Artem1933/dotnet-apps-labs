using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public class SensorEvaluatorRegistry
{
    private readonly Dictionary<SensorType, ISensorEvaluator> _evaluators = new();

    public void Register(SensorType type, ISensorEvaluator evaluator)
    {
        _evaluators[type] = evaluator;
    }

    public void Register(
        SensorType type,
        Func<double, SafetyStatus> evaluatorFunc)
    {
        Register(type, new FuncSensorEvaluator(evaluatorFunc));
    }

    public SafetyStatus Evaluate(SensorType type, double value)
    {
        if (!_evaluators.TryGetValue(type, out var evaluator))
        {
            throw new InvalidOperationException(
                $"Немає зареєстрованого оцінювача для типу {type}");
        }

        return evaluator.Evaluate(value);
    }
}