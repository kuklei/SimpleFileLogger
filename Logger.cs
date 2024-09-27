using System;
using System.IO;

namespace SimpleFileLogger
{
    public static class FileLogger
    {
        private static string logFilePath;
        private static bool enableLogging;
        private static LogLevel minimumLogLevel;
        private static bool isInitialized = false;

        public static void Initialize(LoggerConfig config)
        {
            if (isInitialized)
            {
                throw new InvalidOperationException("FileLogger is already initialized.");
            }

            logFilePath = Path.Combine(config.LogFilePath, GenerateFileName());
            enableLogging = config.EnableLogging;
            minimumLogLevel = config.MinimumLogLevel;

            if (enableLogging)
            {
                string directoryPath = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            isInitialized = true;
        }

        private static void EnsureInitialized()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("FileLogger is not initialized. Call Initialize() before using the logger.");
            }
        }

        private static void Log(LogLevel level, string message)
        {
            EnsureInitialized();

            if (!enableLogging || level < minimumLogLevel)
                return;

            try
            {
                string logMessage = $"- {DateTime.Now:HH:mm:ss} [{level}] - {message}";
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log message: {ex.Message}");
            }
        }

        public static void Debug(string message) => Log(LogLevel.Debug, message);

        public static void Info(string message) => Log(LogLevel.Info, message);

        public static void Warning(string message) => Log(LogLevel.Warning, message);

        public static void Error(string message) => Log(LogLevel.Error, message);

        public static void Critical(string message) => Log(LogLevel.Critical, message);

        private static string GenerateFileName()
        {
            return $"log_{DateTime.Now:yyyyMMdd}.txt";
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    public class LoggerConfig
    {
        public string LogFilePath { get; set; }
        public bool EnableLogging { get; set; }
        public LogLevel MinimumLogLevel { get; set; }
    }
}