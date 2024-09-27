# SimpleFileLogger

```
class Program
{
    static void Main(string[] args)
    {
        // Initialize the logger
        var config = new LoggerConfig
        {
            LogDirectory = @"C:\Logs",
            EnableLogging = true,
            MinimumLogLevel = LogLevel.Debug,
            MaxLogFiles = 30,
            MaxLogFileSizeMB = 1
        };
        FileLogger.Initialize(config);

        // Use the logger
        FileLogger.Info("Application started");
        FileLogger.Debug("This is a debug message");
        FileLogger.Error("An error occurred");
    }
}
```

Or you can load from a json config file using Newtonsoft.Json (or System.Text,Json which is more straightforward)

```
try
    {
        using (StreamReader file = File.OpenText(jsonConfigFilePath))
        using (JsonTextReader reader = new JsonTextReader(file))

        {
            JsonSerializer serializer = new JsonSerializer();
            var config = serializer.Deserialize<LoggerConfig>(reader);
            FileLogger.Initialize(config);
        }
    }
catch (Exception ex)
    {
        Console.WriteLine($"Failed to load configuration: {ex.Message}");
        // Set default values
        LogDirectory = Path.Combine(@".\Logs", GenerateFileName());
        enableLogging = true;
        minimumLogLevel = LogLevel.Info;
    }
```