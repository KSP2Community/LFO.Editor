namespace LFO.Shared
{
    public interface ILogger
    {
        public void LogInfo(object message);
        public void LogDebug(object message);
        public void LogWarning(object message);
        public void LogError(object message);
    }
}