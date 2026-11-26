using IoTTelemetryLab.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTTelemetryLab.Data;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly AppDbContext _context;

    public TelemetryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Device> AddDeviceAsync(string name, CancellationToken ct = default)
    {
        var device = new Device { Name = name };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync(ct);
        return device;
    }

    public async Task AddReadingsAsync(int deviceId, IEnumerable<TelemetryRecord> readings,
        CancellationToken ct = default)
    {
        var list = readings.ToList();
        foreach (var r in list)
        {
            r.DeviceId = deviceId;
        }

        _context.Records.AddRange(list);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<Device>> GetAllDevicesAsync(CancellationToken ct = default)
        => await _context.Devices
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .ToListAsync(ct);

    public async Task<Device?> GetDeviceWithRecordsAsync(int id, CancellationToken ct = default)
        => await _context.Devices
            .Include(d => d.Records)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<int> CountRecordsAsync(int deviceId, CancellationToken ct = default)
        => await _context.Records.CountAsync(r => r.DeviceId == deviceId, ct);

    public async Task<List<DeviceSummary>> GetSummariesAsync(CancellationToken ct = default)
        => await _context.Devices
            .OrderBy(d => d.Id)
            .Select(d => new DeviceSummary(d.Id, d.Name, d.Records.Count))
            .ToListAsync(ct);

    public async Task<bool> RenameDeviceAsync(int id, string newName, CancellationToken ct = default)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (device is null) return false;

        device.Name = newName;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteDeviceAsync(int id, CancellationToken ct = default)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (device is null) return false;

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<AxisMaximum>> GetAxisMaximumsAsync(CancellationToken ct = default)
        => await _context.Records
            .GroupBy(r => r.Axis)
            .OrderByDescending(g => g.Max(r => r.Value))
            .Select(g => new AxisMaximum(g.Key, g.Max(r => r.Value)))
            .ToListAsync(ct);

    public async Task<List<DeviceAxisSummary>> GetAxisSummariesAsync(
        string axis, double threshold, CancellationToken ct = default)
        => await _context.Devices
            .OrderBy(d => d.Id)
            .Select(d => new DeviceAxisSummary(
                d.Id,
                d.Name,
                d.Records.Count(r => r.Axis == axis && r.Value > threshold)))
            .ToListAsync(ct);
}
