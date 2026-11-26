using IoTTelemetryLab.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTTelemetryLab.Data;
public class AppDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetryRecord> Records => Set<TelemetryRecord>();
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>().Property(d => d.Name).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<TelemetryRecord>().Property(r => r.Axis).IsRequired().HasMaxLength(20);
        modelBuilder.Entity<TelemetryRecord>()
            .HasOne(r => r.Device).WithMany(d => d.Records)
            .HasForeignKey(r => r.DeviceId).OnDelete(DeleteBehavior.Cascade);
    }
}
