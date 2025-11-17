# IOTSystem
In IOTSystem repository, a simulated sensor (likely named something like IOTSensor) is designed to generate random humidity data to test IoT data flow. Here’s how it typically works:

Every 10 seconds, the program generates a random humidity value between 30% and 90%.
This data, along with a timestamp, is sent via UDP protocol to two destinations: the Server (on port 5000) and the Client (on port 5001).
This setup helps simulate real-world IoT sensor behavior, allowing both the server and client applications to process and display incoming environmental data as if they were receiving it from a physical sensor.
This simulation is useful for local testing and development of systems that rely on sensor data, without needing the actual hardware present.


1. IoTServer
Receives messages from a RabbitMQ queue named "humidity_queue".
For each message (humidity reading), it:
Deserializes the message (HumidityMessage).
Sends the data to the IoTWebAPI via HTTP POST (/api/humidity).
Prints results and error counts.
It acts as a middleman: RabbitMQ → Web API, making your system scalable and decoupled.
IoTServer/Program.cs

2. IoTWebAPI
ASP.NET Core minimal API that:
Persists humidity readings into an SQLite database via Entity Framework Core.
Provides HTTP endpoints to:
Save new readings (POST /api/humidity)
Fetch the latest readings, basic statistics, and lists of readings.
Has CORS support and Swagger UI for testing/development.
IoTWebAPI/Program.cs

3. IoTClient
Appears to be a WPF (.NET) GUI app.
Periodically fetches humidity reading data and statistics from the API.
Displays:
List of latest humidity data in a DataGrid.
Statistics (average, min/max, total records, latest value).
Allows manual and automatic refreshing.
Uses the /api/humidity and /api/humidity/stats endpoints.
MainWindow.xaml.cs

4. SystemShared
Namespace/folder not directly listed in the results, but referenced in code as IoTSystem.Shared;.
Contains models like HumidityMessage and HumidityData, which are used across all projects for consistent data structures.
Component flow:

IoTSensor (simulator) → [RabbitMQ] → IoTServer → [HTTP] → IoTWebAPI (stores to DB) ←→ IoTClient (displays)
