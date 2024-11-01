using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFileLogger
{
    public static class SFL
    {
        //ensure only one instance of the logger is initialized
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private static string logDirectory;
        private static bool enableLogging;
        private static LogLevel minimumLogLevel;
        private static int maxLogFiles;
        private static long maxFileSizeBytes;
        private static bool isInitialized = false;
        private static string currentLogFile;
        private static object fileLock = new object();

        public static async Task InitializeAsync(SFLConfig config)
        {

            await _semaphore.WaitAsync();
            try
            {
                if (isInitialized)
                {
                    return;
                }

                logDirectory = config.LogDirectory;
                enableLogging = config.EnableLogging;
                minimumLogLevel = config.MinimumLogLevel;
                maxLogFiles = config.MaxLogFiles;
                maxFileSizeBytes = config.MaxFileSizeMB * 1024 * 1024;

                if (enableLogging)
                {
                    try
                    {
                        if (!Directory.Exists(logDirectory))
                        {
                            Directory.CreateDirectory(logDirectory);
                        }
                    }
                    catch (Exception)
                    {
                        enableLogging = false;
                        Console.WriteLine("Failed to create log directory for SFL. Logging will be disabled.");
                    }

                }
                currentLogFile = GetLogFilePath();
                isInitialized = true;

                CleanupOldLogs();
            }
            finally
            {
                _semaphore.Release();
            }

        }

        public static void Initialize(SFLConfig config)
        {
            InitializeAsync(config).GetAwaiter().GetResult();
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
                lock (fileLock)
                {
                    string todayLogFile = GetLogFilePath();
                    if (todayLogFile != currentLogFile)
                    {
                        currentLogFile = todayLogFile;
                        CleanupOldLogs();
                    }

                    if (File.Exists(currentLogFile) && new FileInfo(currentLogFile).Length + logMessage.Length > maxFileSizeBytes)
                    {
                        currentLogFile = GetLogFilePath(true);
                    }

                    File.AppendAllText(currentLogFile, logMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log message: {ex.Message}");
            }
        }

        private static string GetLogFilePath(bool split = false)
        {
            string fileName = $"log_{DateTime.Now:yyyyMMdd}";
            if (split)
            {
                fileName += $"_{DateTime.Now:HHmmss}";
            }
            fileName += ".txt";
            return Path.Combine(logDirectory, fileName);
        }

        private static void CleanupOldLogs()
        {
            if (!Directory.Exists(logDirectory))
            {
                return;
            }
            var logFiles = Directory.GetFiles(logDirectory, "log_*.txt")
                                    .OrderByDescending(f => f)
                                    .Skip(maxLogFiles)
                                    .ToList();

            foreach (var file in logFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete old log file {file}: {ex.Message}");
                }
            }
        }

        public static void Debug(string message) => Log(LogLevel.Debug, message);

        public static void Info(string message) => Log(LogLevel.Info, message);

        public static void Warning(string message) => Log(LogLevel.Warning, message);

        public static void Error(string message) => Log(LogLevel.Error, message);

        public static void Critical(string message) => Log(LogLevel.Critical, message);

        private static string GenerateFileName()
        {
            return $"SFLog_{DateTime.Now:yyyyMMdd}.txt";
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

    public class SFLConfig
    {
        public string LogDirectory { get; set; } = @".\SFLogs"; // Default to "logs
        public bool EnableLogging { get; set; } // Default disabled
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Info;
        public int MaxLogFiles { get; set; } = 30; // Default to keep last 30 log files
        public long MaxFileSizeMB { get; set; } = 5; // 5 MB default
    }
}