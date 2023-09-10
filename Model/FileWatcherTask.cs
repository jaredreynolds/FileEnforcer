namespace FileEnforcer.Model;

public record FileWatcherTask(string Source, string Target, FileWatcherAction Action);
