using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FileEnforcer.Configuration;
using FileEnforcer.Model;
using FileEnforcer.Extensions;
using System.Text.Json;

namespace FileEnforcer.Services
{
    public class FileWatcherService : IService
    {
        private readonly Dictionary<FileSystemWatcher, FileWatcherTask> _watchers = new();
        private readonly IEnumerable<FileWatcherTask> _fileWatcherTasks;
        private readonly DebouncerService<FileWatcherTask, FileSystemWatcher> _debouncer;
        private readonly FileEnforcementService _fileEnforcement;
        private readonly ILogger<FileWatcherService> _logger;
        private bool _isDisposed;

        public FileWatcherService(
            IOptions<FileEnforcementOptions> options, FileEnforcementService fileEnforcement, ILogger<FileWatcherService> logger)
        {
            (_fileEnforcement, _logger) = (fileEnforcement, logger);

            _fileWatcherTasks = (options.Value.Tasks ?? Enumerable.Empty<FileWatcherTask>())
                .Where(fwo => fwo.Action != FileWatcherAction.Unknown);
            _logger.LogTrace(() => $"File watcher tasks with known actions: {JsonSerializer.Serialize(_fileWatcherTasks)}");

            _debouncer = new DebouncerService<FileWatcherTask, FileSystemWatcher>(o => o.Target, OnDebouncedFileChanged);
        }

        public void Start()
        {
            var watchers = _fileWatcherTasks
                .Select(task =>
                {
                    _fileEnforcement.Initialize(task);

                    var (directory, filename) = GetPathParts(task.Target);
                    var watcher = new FileSystemWatcher(directory, filename);
                    watcher.Changed += OnFileChanged;
                    watcher.EnableRaisingEvents = true;
                    return new KeyValuePair<FileSystemWatcher, FileWatcherTask>(watcher, task);
                });

            _watchers.AddRange(watchers);
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
