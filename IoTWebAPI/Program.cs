using Microsoft.EntityFrameworkCore;
using IoTSystem.Shared;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
var dbPath = DatabaseConfig.GetDatabasePath();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.EnsureCreated();
Console.WriteLine("[DB] Database ready!");
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.MapGet("/", () => "IoT API Running!");
// ⭐ NEW: Write endpoint for Server
app.MapPost("/api/humidity", async (AppDbContext db, HumidityMessage message) =>
{
    // Validation
    if (message.HumidityValue < 0 || message.HumidityValue > 100)
    {
        return Results.BadRequest(new { Error = "Humidity must be between 0-100%" });
    }
    if (string.IsNullOrEmpty(message.SensorId))
    {
        return Results.BadRequest(new { Error = "SensorId is required" });
    }
    // Transform message to entity
    var data = new HumidityData
    {
        HumidityValue = message.HumidityValue,
        SensorId = message.SensorId,
        Timestamp = message.Timestamp
    };
    // Save to database
    await db.HumidityReadings.AddAsync(data);
    await db.SaveChangesAsync();
    Console.WriteLine($"[API] Saved: {data.HumidityValue:F2}% from {data.SensorId} at {data.Timestamp:HH:mm:ss}");
    return Results.Created($"/api/humidity/{data.Id}", data);
})
.WithName("CreateHumidityReading")
.WithDescription("Save humidity reading from IoT Server")
.WithOpenApi();
// Existing read endpoints
app.MapGet("/api/humidity", async (AppDbContext db) =>
{
    var data = await db.HumidityReadings
        .OrderByDescending(h => h.Timestamp)
        .Take(100)
        .ToListAsync();
    return Results.Ok(data);
}).WithName("GetAllHumidity").WithOpenApi();
app.MapGet("/api/humidity/latest", async (AppDbContext db) =>
{
    var latest = await db.HumidityReadings
        .OrderByDescending(h => h.Timestamp)
        .FirstOrDefaultAsync();
    return latest != null ? Results.Ok(latest) : Results.NotFound();
}).WithName("GetLatestHumidity").WithOpenApi();
app.MapGet("/api/humidity/stats", async (AppDbContext db) =>
{
    var readings = await db.HumidityReadings.ToListAsync();
    if (!readings.Any())
    {
        return Results.Ok(new { Count = 0, Average = 0.0, Min = 0.0, Max = 0.0, Latest = 0.0,
            LatestTimestamp = DateTime.UtcNow, Message = "No data yet" });
    }
    var stats = new
    {
        Count = readings.Count,
        Average = Math.Round(readings.Average(h => h.HumidityValue), 2),
        Min = Math.Round(readings.Min(h => h.HumidityValue), 2),
        Max = Math.Round(readings.Max(h => h.HumidityValue), 2),
        Latest = Math.Round(readings.OrderByDescending(h => h.Timestamp).First().HumidityValue, 2),
        LatestTimestamp = readings.OrderByDescending(h => h.Timestamp).First().Timestamp
    };
    return Results.Ok(stats);
}).WithName("GetStats").WithOpenApi();
Console.WriteLine("API: http://localhost:5000");
Console.WriteLine("Swagger: http://localhost:5000/swagger");
app.Run();
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<HumidityData> HumidityReadings { get; set; }
}