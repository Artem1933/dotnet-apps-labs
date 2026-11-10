namespace IoTTelemetryLab.Models;

/// <summary>Незмінний показ датчика: ідентифікатор, значення та час вимірювання.</summary>
public readonly record struct TelemetryReading(
    string DeviceId,
    double Value,
    DateTime Timestamp);