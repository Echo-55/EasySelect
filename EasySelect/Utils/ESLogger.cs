using UnityModManagerNet;

namespace EasySelect.Utils
{
    public class ESLogger
    {
        private static ESLogger _instance;
        private static UnityModManager.ModEntry.ModLogger _logger;
        private const string MessagePrefix = "[EasySelect] ";
        private static bool _isDebugMode;

        private ESLogger(UnityModManager.ModEntry.ModLogger logger, bool isDebugMode)
        {
            _logger = logger;
            _isDebugMode = isDebugMode;
        }

        public static void Initialize(UnityModManager.ModEntry.ModLogger logger, bool isDebugMode)
        {
            _instance ??= new ESLogger(logger, isDebugMode);
        }

        public static ESLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new System.InvalidOperationException("ESLogger is not initialized. Call Initialize() first.");
                }

                return _instance;
            }
        }

        public static void Log(string message)
        {
            _logger.Log(MessagePrefix + message);
        }

        public static void LogError(string message)
        {
            var stackFrame = new System.Diagnostics.StackFrame(1);
            var callerMethod = stackFrame.GetMethod().Name;
            var lineNumber = stackFrame.GetFileLineNumber();
            _logger.Error($"{MessagePrefix} ERROR {callerMethod}:{lineNumber}: {message}");
        }

        public static void LogDebug(string message)
        {
            if (!_isDebugMode) return;
            Log(MessagePrefix + message);
        }

        public static void LogDebugError(string message)
        {
            if (!_isDebugMode) return;
            LogError(MessagePrefix + message);
        }

        public static void SetDebugMode(bool state)
        {
            _isDebugMode = state;
            Log($"Logging debug enabled: {_isDebugMode}");
        }
    }
}