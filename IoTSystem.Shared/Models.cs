namespace IoTSystem.Shared;
public class HumidityData
{
    public int Id { get; set; }
    public double HumidityValue { get; set; }
    private DateTime _timestamp;
    public DateTime Timestamp
    {
        get => _timestamp.Kind == DateTimeKind.Utc ? _timestamp.ToLocalTime() : _timestamp;
        set => _timestamp = value;
    }
    public string SensorId { get; set; } = "SENSOR-001";
}
public class HumidityMessage
{
    public double HumidityValue { get; set; }
    public string SensorId { get; set; } = "SENSOR-001";
    public DateTime Timestamp { get; set; }
}