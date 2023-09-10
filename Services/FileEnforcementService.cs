using System.IO.Hashing;
using FileEnforcer.Model;

namespace FileEnforcer.Services;

public class FileEnforcementService
{
    private readonly Dictionary<FileWatcherAction, Action<FileWatcherTask>> _initializations;
    private readonly Dictionary<FileWatcherAction, Action<FileWatcherTask>> _actions;
    private readonly ILogger<FileEnforcementService> _logger;

    public FileEnforcementService(ILogger<FileEnforcementService> logger)
    {
        _logger = logger;

        _initializations = new()
        {
            [FileWatcherAction.Copy] = CopyIfDifferent,
            [FileWatcherAction.Unknown] = _ => { }
        };

        _actions = new()
        {
            [FileWatcherAction.Copy] = Copy,
            [FileWatcherAction.Unknown] = _ => { }
        };
    }

    public void Initialize(FileWatcherTask task)
    {
        _initializations[task.Action](task);
    }

    public void Execute(FileWatcherTask task)
    {
        _actions[task.Action](task);
    }

    private void CopyIfDifferent(FileWatcherTask task)
    {
        var sourceHash = ComputeHashOfFile(task.Source);
        var targetHash = ComputeHashOfFile(task.Target);

        if (!sourceHash.SequenceEqual(targetHash))
        {
            _logger.LogInformation($"Overwriting file '{task.Target}' with '{task.Source}'.");
            File.Copy(task.Source, task.Target, overwrite: true);
        }

    }

    private void Copy(FileWatcherTask task)
    {
        _logger.LogInformation($"Overwriting file '{task.Target}' with '{task.Source}'.");

        File.Copy(task.Source, task.Target, overwrite: true);
    }

    private static byte[] ComputeHashOfFile(string filePath)
    {
        var hashAlgorithm = new XxHash64();
        using (var stream = File.OpenRead(filePath))
        {
            hashAlgorithm.Append(stream);
        }
        return hashAlgorithm.GetHashAndReset();
    }

}
