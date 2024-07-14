namespace Engine;

public static class AppLogger
{
    private const string LogPath = ".";

    public static void Info(string message)
    {
        Log(message, "INFO");
    }

    public static void Error(string message)
    {
        Log(message, "ERROR");
    }

    private static void Log(string message, string level)
    {
        var msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
        var path = $"{LogPath}/{DateTime.Now:yyyy_MM_dd}.log";
        File.AppendAllText(path, msg + Environment.NewLine);
    }
}