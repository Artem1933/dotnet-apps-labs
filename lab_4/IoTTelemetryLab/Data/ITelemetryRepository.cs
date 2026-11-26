// Data/ITelemetryRepository.cs
using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Data;

public interface ITelemetryRepository
{
    Task<Device> AddDeviceAsync(string name, CancellationToken ct = default);
    Task AddReadingsAsync(int deviceId, IEnumerable<TelemetryRecord> readings, CancellationToken ct = default);

    Task<List<Device>> GetAllDevicesAsync(CancellationToken ct = default);
    Task<Device?> GetDeviceWithRecordsAsync(int id, CancellationToken ct = default);
    Task<int> CountRecordsAsync(int deviceId, CancellationToken ct = default);
    Task<List<DeviceSummary>> GetSummariesAsync(CancellationToken ct = default);

    Task<bool> RenameDeviceAsync(int id, string newName, CancellationToken ct = default);
    Task<bool> DeleteDeviceAsync(int id, CancellationToken ct = default);
    Task<List<AxisMaximum>> GetAxisMaximumsAsync(CancellationToken ct = default);
    Task<List<DeviceAxisSummary>> GetAxisSummariesAsync(
        string axis, double threshold, CancellationToken ct = default);
}
