using FileEnforcer.Model;

namespace FileEnforcer.Configuration;

public class FileEnforcementOptions
{
    public IEnumerable<FileWatcherTask> Tasks { get; set; } = Enumerable.Empty<FileWatcherTask>();
}
