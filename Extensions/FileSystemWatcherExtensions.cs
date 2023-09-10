namespace FileEnforcer.Extensions;

public static class FileSystemWatcherExtensions
{
    public static void WithoutEvents(this FileSystemWatcher watcher, Action action)
    {
        if (watcher != null)
        {
            watcher.EnableRaisingEvents = false;
        }

        try
        {
            action?.Invoke();
        }
        finally
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = true;
            }
        }
    }
}
