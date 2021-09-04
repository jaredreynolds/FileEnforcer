using System;
using System.Collections.Generic;
using System.IO;
using FileEnforcer.Model;
using Microsoft.Extensions.Logging;

namespace FileEnforcer
{
    public class FileEnforcement
    {
        private readonly Dictionary<FileWatcherAction, Action<FileWatcherTask>> _actions;
        private readonly ILogger<FileEnforcement> _logger;

        public FileEnforcement(ILogger<FileEnforcement> logger)
        {
            _logger = logger;

            _actions = new()
            {
                [FileWatcherAction.Copy] = Copy,
                [FileWatcherAction.Unknown] = _ => { }
            };
        }

        public void Execute(FileWatcherTask task)
        {
            _actions[task.Action](task);
        }

        private void Copy(FileWatcherTask task)
        {
            _logger.LogInformation($"Overwriting file '{task.Target}' with '{task.Source}'.");

            File.Copy(task.Source, task.Target, overwrite: true);
        }
    }
}
