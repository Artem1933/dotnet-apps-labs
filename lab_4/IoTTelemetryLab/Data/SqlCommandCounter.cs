namespace IoTTelemetryLab.Data;

public static class SqlCommandCounter
{
    public static int Count { get; private set; }

    public static void Reset() => Count = 0;

    public static void OnLog(string message)
    {
        if (message.Contains("Executed DbCommand"))
        {
            Count++;
        }
    }
}
