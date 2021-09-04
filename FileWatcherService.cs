using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using K4os.Hash.xxHash;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FileEnforcer.Configuration;
using FileEnforcer.Model;
using FileEnforcer.Extensions;
using System.Text.Json;

namespace FileEnforcer
{
    public class FileWatcherService : IService
    {
        private static readonly HashAlgorithm _hashfn = new XXH64().AsHashAlgorithm();

        private readonly Dictionary<FileSystemWatcher, FileWatcherTask> _watchers = new();
        private readonly IEnumerable<FileWatcherTask> _fileWatcherTasks;
        private readonly Debouncer<FileWatcherTask, FileSystemWatcher> _debouncer;
        private readonly FileEnforcement _fileEnforcement;
        private readonly ILogger<FileWatcherService> _logger;
        private bool _isDisposed;

        public FileWatcherService(
            IOptions<FileEnforcementOptions> options, FileEnforcement fileEnforcement, ILogger<FileWatcherService> logger)
        {
            (_fileEnforcement, _logger) = (fileEnforcement, logger);

            _fileWatcherTasks = (options.Value.Tasks ?? Enumerable.Empty<FileWatcherTask>())
                .Where(fwo => fwo.Action != FileWatcherAction.Unknown);
            _logger.LogTrace(() => $"File watcher tasks with known actions: {JsonSerializer.Serialize(_fileWatcherTasks)}");

            _debouncer = new Debouncer<FileWatcherTask, FileSystemWatcher>(o => o.Target, OnDebouncedFileChanged);
        }

        public void Start()
        {
            UpdateAllFiles();

            var watchers = _fileWatcherTasks
                .Select(task =>
                {
                    var (directory, filename) = GetPathParts(task.Target);
                    var watcher = new FileSystemWatcher(directory, filename);
                    watcher.Changed += OnFileChanged;
                    watcher.EnableRaisingEvents = true;
                    return new KeyValuePair<FileSystemWatcher, FileWatcherTask>(watcher, task);
                });

            _watchers.AddRange(watchers);
        }

        private void UpdateAllFiles()
        {
            _logger.LogTrace("Updating all files.");

            _fileWatcherTasks.ForEach(task =>
            {
                var sourceHash = _hashfn.ComputeHash(File.ReadAllBytes(task.Source));
                var targetHash = _hashfn.ComputeHash(File.ReadAllBytes(task.Target));

                if (!sourceHash.SequenceEqual(targetHash))
                {
                    _logger.LogInformation($"Overwriting file '{task.Target}' with '{task.Source}'.");
                    File.Copy(task.Source, task.Target, overwrite: true);
                }
            });
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            _logger.LogDebug(() => $"File {e.ChangeType.ToString().ToLower()}: '{e.FullPath}'.");

            var watcher = (FileSystemWatcher)sender;
            var options = _watchers[watcher];
            _debouncer.Debounce(options, watcher);
        }

        private void OnDebouncedFileChanged(object key, FileWatcherTask task, FileSystemWatcher watcher)
        {
            _logger.LogTrace(() => $"File change for {task.Target} has been debounced.");

            watcher.WithoutEvents(() => _fileEnforcement.Execute(task));
        }

        private static (string, string) GetPathParts(string path)
            => (Path.GetDirectoryName(path), Path.GetFileName(path));

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _watchers.Keys.ForEach(w => w.Dispose());
                }

                _watchers.Clear();
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
