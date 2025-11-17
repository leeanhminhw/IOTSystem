using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using IoTSystem.Shared;
Console.WriteLine("=== IoT Server  ===");
Console.WriteLine("Sending data to API instead of direct DB write\n");
// ⭐ HttpClient để gọi API
var httpClient = new HttpClient
{
    BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000"),
    Timeout = TimeSpan.FromSeconds(10)
};
// Test API connection
try
{
    var healthCheck = await httpClient.GetStringAsync("/");
    Console.WriteLine($" API connected: {healthCheck}");
}
catch (Exception ex)
{
    Console.WriteLine($"  Warning: Cannot connect to API - {ex.Message}");
    Console.WriteLine("Make sure IoTWebAPI is running first!");
}
// Connect to RabbitMQ
var factory = new ConnectionFactory()
{
    HostName = Environment.GetEnvironmentVariable("RabbitMq__HostName") ?? "localhost"
};
var connection = await factory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();
await channel.QueueDeclareAsync(
    queue: "humidity_queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);
Console.WriteLine(" Connected to RabbitMQ");
Console.WriteLine("Listening for messages...\n");
// Consumer
var consumer = new AsyncEventingBasicConsumer(channel);
int successCount = 0;
int errorCount = 0;
consumer.ReceivedAsync += async (model, ea) =>
{
    try
    {
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);
        var message = JsonSerializer.Deserialize<HumidityMessage>(json);
        if (message != null)
        {
            // ⭐ Gọi API 
            var response = await httpClient.PostAsJsonAsync("/api/humidity", message);
            if (response.IsSuccessStatusCode)
            {
                successCount++;
                var savedData = await response.Content.ReadFromJsonAsync<HumidityData>();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]  Saved via API: {message.HumidityValue:F2}% (ID: {savedData?.Id})");
                Console.WriteLine($"    Success: {successCount}  Errors: {errorCount}\n");
            }
            else
            {
                errorCount++;
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]  API Error ({response.StatusCode}): {errorContent}");
                Console.WriteLine($"    Success: {successCount}  Errors: {errorCount}\n");
            }
        }
        await channel.BasicAckAsync(ea.DeliveryTag, false);
    }
    catch (HttpRequestException ex)
    {
        errorCount++;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]  Network Error: {ex.Message}");
        Console.WriteLine($"   Success: {successCount}  Errors: {errorCount}\n");
        // Don't ACK - message will be redelivered
    }
    catch (Exception ex)
    {
        errorCount++;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]  Error: {ex.Message}");
        Console.WriteLine($"   Success: {successCount}  Errors: {errorCount}\n");
        await channel.BasicAckAsync(ea.DeliveryTag, false);
    }
};
await channel.BasicConsumeAsync(
    queue: "humidity_queue",
    autoAck: false,
    consumer: consumer
);
Console.WriteLine("Press [Enter] to exit...");
Console.ReadLine();