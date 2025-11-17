using RabbitMQ.Client; 
using System.Text; 
using System.Text.Json;
using IoTSystem.Shared;
Console.WriteLine("=== IoT Sensor ===\nSending every 10s...\n");
var factory = new ConnectionFactory() 
{
    HostName = Environment.GetEnvironmentVariable("RabbitMq__HostName") ?? "localhost" 
};
await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();
await channel.QueueDeclareAsync("humidity_queue", true, false, false, null);
var random = new Random(); int count = 0;
using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
//tao gia tri random tu 30-90
while (await timer.WaitForNextTickAsync()) {
    count++; var humidity = 30 + random.NextDouble() * 60;
    var message = new HumidityMessage {
        HumidityValue = Math.Round(humidity, 2),
        SensorId = "SENSOR-001", Timestamp = DateTime.UtcNow 
    };
    var json = JsonSerializer.Serialize(message);
    var body = Encoding.UTF8.GetBytes(json);
    var props = new BasicProperties
    { 
        DeliveryMode = DeliveryModes.Persistent
    };
    await channel.BasicPublishAsync("", "humidity_queue", false, props, body);
    Console.WriteLine($"[{count}] {DateTime.Now:HH:mm:ss} - Sent: {humidity:F2}%");
}