using System.Diagnostics;
using Serilog;

namespace EasySelect.Utils;

public static class ESLogger
{
    private const string MessagePrefix = "[EasySelect] ";
    private static ELogLevel _currentLogLevel = ELogLevel.Info;
    private static readonly ILogger Logger = Serilog.Log.ForContext<EasySelectController>();

    public static void Log(string message)
    {
        if (_currentLogLevel == ELogLevel.NONE || _currentLogLevel > ELogLevel.Info) return;
        Logger.Information($"{MessagePrefix}: {message}");
        Main.ModEntry.Logger.Log($"{MessagePrefix}: {message}");
    }

    public static void LogError(string message)
    {
        if (_currentLogLevel == ELogLevel.NONE || _currentLogLevel > ELogLevel.Error) return;
        var stackFrame = new StackFrame(1);
        var callerMethod = stackFrame.GetMethod().Name;
        var lineNumber = stackFrame.GetFileLineNumber();
        Logger.Error($"{MessagePrefix}: ERROR: {callerMethod}:{lineNumber}: {message}");
        Main.ModEntry.Logger.Error($"{MessagePrefix}: ERROR: {callerMethod}:{lineNumber}: {message}");
    }

    public static void LogDebug(string message)
    {
        if (_currentLogLevel == ELogLevel.NONE || _currentLogLevel > ELogLevel.Debug) return;
        Logger.Debug($"{MessagePrefix} DEBUG: {message}");
        Log(message);
    }

    public static void LogDebugError(string message)
    {
        if (_currentLogLevel == ELogLevel.NONE || _currentLogLevel > ELogLevel.Debug) return;
        Logger.Error($"{MessagePrefix} DEBUG ERROR: {message}");
        LogError(message);
    }

    public static void SetLogLevel(ELogLevel logLevel)
    {
        _currentLogLevel = logLevel;
        Logger.Information($"Log level set to {logLevel}");
        Log($"Log level set to {logLevel}");
    }

    public enum ELogLevel
    {
        NONE,
        Info,
        Debug,
        Error
    }
}