using IoTTelemetryLab.Data;
namespace IoTTelemetryLab.Processing;

public class AxisMaximum
{
    public string Axis { get; set; } = string.Empty;
    public double Maximum { get; set; }
}
public static class AxisAnalytics
{
    public static List<AxisMaximum> GetMaximums(AppDbContext context)
    {
        return context.Records
            .GroupBy(r => r.Axis)
            .Select(g => new AxisMaximum { Axis = g.Key, Maximum = g.Max(r => r.Value) })
            .OrderByDescending(r => r.Maximum)
            .ToList();
    }
}
