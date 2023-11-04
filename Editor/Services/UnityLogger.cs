using UnityEngine;
using ILogger = LFO.Shared.ILogger;

namespace LFO.Editor.Services
{
    public class UnityLogger : ILogger
    {
        private const string Prefix = "[LFO]";

        public void LogInfo(object message)
        {
            Debug.Log($"{Prefix} {message}");
        }

        public void LogDebug(object message)
        {
            Debug.Log($"{Prefix} {message}");
        }

        public void LogWarning(object message)
        {
            Debug.LogWarning($"{Prefix} {message}");
        }

        public void LogError(object message)
        {
            Debug.LogError($"{Prefix} {message}");
        }
    }
}