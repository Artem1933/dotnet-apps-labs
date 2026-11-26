using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Processing;

/// <summary>Очищає телеметрію від апаратних помилок.</summary>
public static class TelemetryCleaner
{
    private const double ErrorMarker = -999.0;

    /// <summary>Повертає послідовність показів без NaN та маркера помилки -999.</summary>
    public static IEnumerable<TelemetryReading> RemoveInvalid(
        IEnumerable<TelemetryReading> raw)
    {
        return raw.Where(r =>
            !double.IsNaN(r.Value) &&
            r.Value != ErrorMarker);
    }
}