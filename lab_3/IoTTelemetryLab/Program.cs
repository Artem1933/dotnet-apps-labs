using System.Globalization;
using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Models;
using IoTTelemetryLab.Processing;
using Microsoft.EntityFrameworkCore;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
using var context = new AppDbContext();
context.Database.Migrate();

Console.WriteLine("Лабораторна робота №3. Варіант 2 - вібрація, мм/с");
Console.WriteLine($"База даних: {AppDbContext.DatabasePath}");
while (true)
{
    Console.WriteLine("\n1 - Додати сенсор і 50 показів");
    Console.WriteLine("2 - Перелік сенсорів");
    Console.WriteLine("3 - Переглянути покази сенсора");
    Console.WriteLine("4 - Перейменувати сенсор");
    Console.WriteLine("5 - Видалити сенсор разом із показами");
    Console.WriteLine("6 - Видалити покази за умовою");
    Console.WriteLine("7 - Максимум вібрації по кожній осі");
    Console.WriteLine("8 - Наповнити базу п'ятьма сенсорами");
    Console.WriteLine("0 - Вихід");
    Console.Write("Ваш вибір: ");
    var choice = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(choice) || choice == "0") break;
    try
    {
        switch (choice)
        {
            case "1": AddDevice(); break;
            case "2": ListDevices(); break;
            case "3": ReadRecords(); break;
            case "4": RenameDevice(); break;
            case "5": DeleteDevice(); break;
            case "6": DeleteRecords(); break;
            case "7": PrintMaximums(); break;
            case "8": SeedFive(); break;
            default: Console.WriteLine("Оберіть пункт від 0 до 8."); break;
        }
    }
    catch (ArgumentException ex) { Console.WriteLine(ex.Message); }
    catch (DbUpdateException) { Console.WriteLine("Не вдалося зберегти зміни до бази."); context.ChangeTracker.Clear(); }
}

void AddDevice()
{
    Console.Write("Назва сенсора (порожній рядок - скасувати): ");
    var name = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(name)) return;
    var count = DeviceSeeder.SeedDevice(context, name);
    Console.WriteLine($"Сенсор '{name}': згенеровано 50, збережено {count}, вилучено {50 - count}.");
}

void SeedFive()
{
    for (int i = 1; i <= 5; i++)
    {
        var name = $"sensor-{i:D2}";
        if (context.Devices.Any(d => d.Name == name))
        {
            Console.WriteLine($"'{name}' уже є в базі, пропускаємо.");
            continue;
        }
        var count = DeviceSeeder.SeedDevice(context, name);
        Console.WriteLine($"'{name}': згенеровано 50, збережено {count}, вилучено {50 - count}.");
    }
}

void ListDevices()
{
    var devices = context.Devices.OrderBy(d => d.Id).ToList();
    if (devices.Count == 0) Console.WriteLine("Немає даних: сенсори відсутні.");
    foreach (var d in devices)
        Console.WriteLine($"[{d.Id}] {d.Name}, показів: {context.Records.Count(r => r.DeviceId == d.Id)}");
}

Device? SelectDevice()
{
    ListDevices();
    Console.Write("Id сенсора (порожній рядок - скасувати): ");
    var text = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(text)) return null;
    if (!int.TryParse(text, out int id))
    {
        Console.WriteLine("Id має бути цілим числом.");
        return null;
    }
    var device = context.Devices.FirstOrDefault(d => d.Id == id);
    if (device is null) Console.WriteLine("Сенсор із таким Id не знайдено.");
    return device;
}

void ReadRecords()
{
    var device = SelectDevice();
    if (device is null) return;
    var records = context.Records.Where(r => r.DeviceId == device.Id)
        .OrderBy(r => r.Timestamp).ThenBy(r => r.Id).ToList();
    if (records.Count == 0) Console.WriteLine("Немає даних.");
    foreach (var r in records)
        Console.WriteLine($"{r.Id,5} | {r.Timestamp:yyyy-MM-dd HH:mm:ss} UTC | {r.Axis} | {r.Value,6:F2} мм/с");
    Console.WriteLine($"Усього показів: {records.Count}");
}

void RenameDevice()
{
    var device = SelectDevice();
    if (device is null) return;
    Console.Write("Нова назва (порожній рядок - скасувати): ");
    var name = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(name)) return;
    if (name.Length > 100) { Console.WriteLine("Максимум 100 символів."); return; }
    if (name == device.Name) { Console.WriteLine("Назва не змінилася."); return; }
    if (context.Devices.Any(d => d.Id != device.Id && d.Name == name))
    { Console.WriteLine("Така назва вже існує."); return; }
    var oldName = device.Name;
    device.Name = name;
    context.SaveChanges();
    Console.WriteLine($"Перейменовано: '{oldName}' -> '{name}'.");
}

bool Confirm(string message)
{
    Console.Write($"{message} (y/n): ");
    return string.Equals(Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
}

void DeleteDevice()
{
    var device = SelectDevice();
    if (device is null) return;
    int id = device.Id;
    int count = context.Records.Count(r => r.DeviceId == id);
    if (!Confirm($"Видалити '{device.Name}' та {count} показів?"))
    { Console.WriteLine("Видалення скасовано."); return; }
    int before = context.Records.Count();
    context.Devices.Remove(device);
    context.SaveChanges();
    Console.WriteLine($"Видалено показів: {before - context.Records.Count()}.");
    Console.WriteLine($"Показів сенсора залишилось: {context.Records.Count(r => r.DeviceId == id)}.");
    Console.WriteLine($"Записів-сиріт: {context.Records.Count(r => !context.Devices.Any(d => d.Id == r.DeviceId))}.");
}

bool ReadNumber(string prompt, out double value)
{
    Console.Write(prompt);
    var text = Console.ReadLine()?.Trim().Replace(',', '.');
    bool valid = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
        && double.IsFinite(value);
    if (!valid) Console.WriteLine("Потрібне скінченне число, наприклад 2.5.");
    return valid;
}

void DeleteRecords()
{
    ListDevices();
    Console.Write("Id сенсора (0 - усі, порожній рядок - скасувати): ");
    var text = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(text)) return;
    if (!int.TryParse(text, out int id) || id < 0)
    { Console.WriteLine("Потрібне ціле невід'ємне число."); return; }
    IQueryable<TelemetryRecord> query = context.Records;
    if (id != 0)
    {
        if (!context.Devices.Any(d => d.Id == id))
        { Console.WriteLine("Сенсор не знайдено."); return; }
        query = query.Where(r => r.DeviceId == id);
    }
    Console.WriteLine("1 - нижче порога; 2 - вище порога; 3 - у діапазоні; 4 - раніше дати UTC");
    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            if (!ReadNumber("Поріг: ", out double below)) return;
            query = query.Where(r => r.Value < below); break;
        case "2":
            if (!ReadNumber("Поріг: ", out double above)) return;
            query = query.Where(r => r.Value > above); break;
        case "3":
            if (!ReadNumber("Нижня межа: ", out double min)) return;
            if (!ReadNumber("Верхня межа: ", out double max)) return;
            if (min > max) { Console.WriteLine("Нижня межа більша за верхню."); return; }
            query = query.Where(r => r.Value >= min && r.Value <= max); break;
        case "4":
            Console.Write("Дата UTC (yyyy-MM-dd або yyyy-MM-dd HH:mm): ");
            if (!DateTime.TryParseExact(Console.ReadLine()?.Trim(),
                new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm" }, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
            { Console.WriteLine("Неправильний формат дати."); return; }
            query = query.Where(r => r.Timestamp < date); break;
        default: Console.WriteLine("Невідома умова."); return;
    }
    int matched = query.Count();
    if (matched == 0) { Console.WriteLine("Немає даних за цією умовою."); return; }
    Console.WriteLine($"Знайдено показів: {matched}. Перші п'ять:");
    foreach (var r in query.OrderBy(r => r.Timestamp).ThenBy(r => r.Id).Take(5).ToList())
        Console.WriteLine($"Id={r.Id}, сенсор={r.DeviceId}, {r.Timestamp:u}, {r.Axis}, {r.Value:F2}");
    if (!Confirm($"Видалити {matched} показів?"))
    { Console.WriteLine("Видалення скасовано."); return; }
    var rows = query.ToList();
    context.Records.RemoveRange(rows);
    context.SaveChanges();
    Console.WriteLine($"Видалено: {rows.Count}. За умовою залишилось: {query.Count()}.");
    Console.WriteLine("Сенсори залишилися в базі.");
}

void PrintMaximums()
{
    var results = AxisAnalytics.GetMaximums(context);
    Console.WriteLine("\nМаксимум вібрації по кожній осі (за спаданням)");
    if (results.Count == 0) { Console.WriteLine("Немає даних."); return; }
    Console.WriteLine("Вісь | Максимум, мм/с");
    foreach (var row in results)
        Console.WriteLine($"{row.Axis,4} | {row.Maximum,14:F2}");
}
