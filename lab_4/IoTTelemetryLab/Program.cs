// Program.cs - змінити файл, крок 10
using System.Globalization;
using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

// ===== ФАЗА 1. РЕЄСТРАЦІЯ СЛУЖБ =====

// [1] Builder: створює конфігурацію (appsettings.json, змінні середовища,
//     аргументи), журналювання та порожній контейнер builder.Services
var builder = Host.CreateApplicationBuilder(args);

// [2] Рядок підключення з appsettings.json; шлях до файлу бази робимо
//     абсолютним (відносно папки з .exe)
var csb = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Не знайдено ConnectionStrings:Default в appsettings.json"));
csb.DataSource = Path.Combine(AppContext.BaseDirectory, csb.DataSource);

// [3] AppDbContext: як створювати контекст (замінює колишній OnConfiguring)
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlite(csb.ConnectionString)
    .LogTo(SqlCommandCounter.OnLog,
           new[] { DbLoggerCategory.Database.Command.Name },
           LogLevel.Information));

// [4] Репозиторій: "хто просить ITelemetryRepository, отримує TelemetryRepository"
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();

// [5] Налаштування: секція "Telemetry" з JSON -> клас TelemetrySettings (Крок 8)
builder.Services.Configure<TelemetrySettings>(builder.Configuration.GetSection("Telemetry"));

// ===== ФАЗА 2. ПОБУДОВА =====

// [6] Реєстрації зафіксовано, контейнер створено
using var host = builder.Build();

// ===== ФАЗА 3. ВИКОРИСТАННЯ =====

// [7] Scoped-служби живуть у межах scope. У вебзастосунку кожен HTTP-запит
//     отримує свій scope автоматично, у консольному створюємо один вручну
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

// [8] Створюємо базу й застосовуємо міграції (не залежить від dotnet ef database update)
await services.GetRequiredService<AppDbContext>().Database.MigrateAsync();

// [9] Просимо в контейнера готові об'єкти
var repository = services.GetRequiredService<ITelemetryRepository>();
var settings = services.GetRequiredService<IOptions<TelemetrySettings>>().Value;

// [10] Наповнення (лише якщо база порожня), параметри беремо з конфігурації
if ((await repository.GetAllDevicesAsync()).Count == 0)
{
    foreach (var name in new[] { "sensor-07", "sensor-12", "sensor-19" })
    {
        var saved = await DeviceSeeder.SeedAsync(
            repository, name, settings.ReadingCount, settings.MinValue, settings.MaxValue);
        Console.WriteLine($"Створено '{name}'. Згенеровано: {settings.ReadingCount}, збережено: {saved}");
    }
}

// === Демонстрація 1: N+1, окремий запит на кожен пристрій ===
Console.WriteLine("\n--- N+1 ---");
SqlCommandCounter.Reset();
var devices = await repository.GetAllDevicesAsync();
foreach (var d in devices)
{
    var count = await repository.CountRecordsAsync(d.Id);
    Console.WriteLine($"  {d.Name}: {count}");
}
Console.WriteLine($"  SQL-команд: {SqlCommandCounter.Count}");

// === Демонстрація 2: проєкція, той самий результат одним запитом ===
Console.WriteLine("\n--- Проєкція ---");
SqlCommandCounter.Reset();
var summaries = await repository.GetSummariesAsync();
foreach (var s in summaries)
{
    Console.WriteLine($"  {s.Name}: {s.RecordsCount}");
}
Console.WriteLine($"  SQL-команд: {SqlCommandCounter.Count}");

// === Демонстрація 3: Include, пристрій разом з усіма показами ===
Console.WriteLine("\n--- Include ---");
SqlCommandCounter.Reset();
var withRecords = await repository.GetDeviceWithRecordsAsync(summaries[0].Id);
Console.WriteLine($"  '{withRecords?.Name}': показів {withRecords?.Records.Count}, SQL-команд: {SqlCommandCounter.Count}");

// === Демонстрація 4: CRUD на тимчасовому пристрої (стан бази не змінюється) ===
Console.WriteLine("\n--- CRUD ---");
var temp = await repository.AddDeviceAsync("temp-sensor");
await repository.RenameDeviceAsync(temp.Id, "temp-sensor-renamed");
Console.WriteLine($"  Видалено: {await repository.DeleteDeviceAsync(temp.Id)}");

// === Варіант 2. Завдання А: максимум вібрації за осями ===
Console.WriteLine("\n--- Варіант 2. Максимум за осями ---");
SqlCommandCounter.Reset();
var maximums = await repository.GetAxisMaximumsAsync();
if (maximums.Count == 0)
{
    Console.WriteLine("Немає даних.");
}
else
{
    Console.WriteLine("Вісь | Максимум, мм/с");
    foreach (var row in maximums)
    {
        Console.WriteLine($"  {row.Axis} | {row.Maximum:F2}");
    }
}
Console.WriteLine($"  SQL-команд: {SqlCommandCounter.Count}");

// === Завдання Б: проєкція кількості показів за віссю та порогом ===
Console.Write("\nВведіть вісь (X/Y/Z): ");
var axis = Console.ReadLine()?.Trim().ToUpperInvariant();
if (axis is not ("X" or "Y" or "Z"))
{
    Console.WriteLine("Потрібно ввести X, Y або Z.");
    return;
}

Console.Write("Введіть поріг вібрації, мм/с: ");
var thresholdInput = Console.ReadLine()?.Trim().Replace(',', '.');
if (!double.TryParse(thresholdInput, NumberStyles.Float,
    CultureInfo.InvariantCulture, out var threshold) || !double.IsFinite(threshold))
{
    Console.WriteLine("Потрібно ввести скінченне число.");
    return;
}

Console.WriteLine($"\n--- Вісь {axis}, значення вище {threshold:F2} мм/с ---");
SqlCommandCounter.Reset();
var axisSummaries = await repository.GetAxisSummariesAsync(axis, threshold);
if (axisSummaries.Count == 0)
{
    Console.WriteLine("Немає даних.");
}
else
{
    Console.WriteLine("Id | Назва | Кількість показів");
    foreach (var row in axisSummaries)
    {
        Console.WriteLine($"  {row.Id} | {row.Name} | {row.RecordsCount}");
    }
}
Console.WriteLine($"  SQL-команд: {SqlCommandCounter.Count}");
