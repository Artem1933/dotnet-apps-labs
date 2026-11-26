using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Evaluation;

public static class EvaluatorSetup
{
    public static SensorEvaluatorRegistry CreateDefaultRegistry()
    {
        var registry = new SensorEvaluatorRegistry();

        registry.Register(
            SensorType.Temperature,
            new TemperatureEvaluator());

        registry.Register(
            SensorType.Vibration,
            new VibrationEvaluator());

        registry.Register(
            SensorType.Voltage,
            new VoltageEvaluator());

        return registry;
    }
}