namespace IoTSystem.Shared;

public static class DatabaseConfig
{
    public static string GetDatabasePath()
    {
        var envPath = Environment.GetEnvironmentVariable("IOT_DB_PATH");
        if (!string.IsNullOrEmpty(envPath))
        {
            var dir = Path.GetDirectoryName(envPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return envPath;
        }
        
        var baseDir = AppContext.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var dataDir = Path.Combine(solutionRoot, "data");
        Directory.CreateDirectory(dataDir);
        
        var dbFile = Path.Combine(dataDir, "iot_data.db");
        Console.WriteLine($"[DB] Using: {dbFile}");
        
        return dbFile;
    }
}