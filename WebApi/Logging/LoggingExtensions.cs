namespace WebApi.Logging;

internal static class LoggingExtensions
{
    public static void LogMemoryUsage(this ILogger logger, string message = null)
    {
        var gcMem = GC.GetTotalMemory(false);
        var memoryUsed = gcMem / 1024.0 / 1024.0;
            
        if (string.IsNullOrWhiteSpace(message))
        {
            logger.LogInformation("Memory usage: {Memory:F2} MB", memoryUsed);
        }
        else
        {
            logger.LogInformation("{Message}. Memory usage: {Memory:F2} MB", message, memoryUsed);
        }
    }
}