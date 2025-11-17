using System.Net.Http; 
using System.Windows;
using System.Windows.Media; 
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using IoTSystem.Shared;
namespace IoTClient;
public class UtcToLocalDateTimeConverter : DateTimeConverterBase
{
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null) return DateTime.MinValue;
        var dt = DateTime.Parse(reader.Value.ToString()!);
        if (dt.Kind == DateTimeKind.Unspecified) dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        return dt.ToLocalTime();
    }
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => writer.WriteValue(value);
}
public class HumidityDataLocal
{
    public int Id { get; set; }
    public string SensorId { get; set; } = "";
    public double HumidityValue { get; set; }
    [JsonConverter(typeof(UtcToLocalDateTimeConverter))]
    public DateTime Timestamp { get; set; }
}
public partial class MainWindow : Window {
    private readonly HttpClient _http = new(); private readonly string _api = "http://localhost:5000/api";
    private DispatcherTimer? _timer; private bool _auto = false;
    public MainWindow() { InitializeComponent(); Loaded += async (s,e) => await Load(); }
    private async void BtnRefresh_Click(object s, RoutedEventArgs e) => await Load();
    private void BtnAutoRefresh_Click(object s, RoutedEventArgs e) {
        _auto = !_auto;
        if (_auto) { _timer = new() { Interval = TimeSpan.FromSeconds(5) }; _timer.Tick += async (s,a) => await Load(); _timer.Start(); BtnAutoRefresh.Content = "Auto ON"; BtnAutoRefresh.Background = new SolidColorBrush(Color.FromRgb(244,67,54)); }
        else { _timer?.Stop(); BtnAutoRefresh.Content = "Auto OFF"; BtnAutoRefresh.Background = new SolidColorBrush(Color.FromRgb(76,175,80)); }
    }
    private async Task Load() {
        try {
            TxtStatus.Text = "Loading..."; TxtStatus.Foreground = new SolidColorBrush(Colors.Orange); BtnRefresh.IsEnabled = false;
            var humidityJson = await _http.GetStringAsync($"{_api}/humidity");
            var data = JsonConvert.DeserializeObject<List<HumidityDataLocal>>(humidityJson);
            var statsJson = await _http.GetStringAsync($"{_api}/humidity/stats");
            var stats = JObject.Parse(statsJson);
            DataGridHumidity.ItemsSource = data;
            var count = stats["Count"]?.Value<int>() ?? 0;
            TxtTotalRecords.Text = count.ToString();
            if (count > 0) {
                TxtAverage.Text = $"{stats["Average"]?.Value<double>() ?? 0:F2}%";
                TxtMinMax.Text = $"{stats["Min"]?.Value<double>() ?? 0:F2}/{stats["Max"]?.Value<double>() ?? 0:F2}";
                TxtLatest.Text = $"{stats["Latest"]?.Value<double>() ?? 0:F2}%";
            } else { TxtAverage.Text = "---"; TxtMinMax.Text = "---/---"; TxtLatest.Text = "---"; }
            TxtStatus.Text = $" {DateTime.Now:HH:mm:ss}"; TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(76,175,80));
        } catch (HttpRequestException ex) {
            TxtStatus.Text = " Connection failed"; TxtStatus.Foreground = new SolidColorBrush(Colors.Red);
            MessageBox.Show($"Cannot connect to API.\n\nError: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        } catch (Exception ex) {
            TxtStatus.Text = " Error"; TxtStatus.Foreground = new SolidColorBrush(Colors.Red);
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        } finally { BtnRefresh.IsEnabled = true; }
    }
}