using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileEnforcer
{
    public class WindowsBackgroundService<T> : BackgroundService where T : IService
    {
        private readonly ILogger<WindowsBackgroundService<T>> _logger;
        private readonly T _service;

        public WindowsBackgroundService(
            T service, ILogger<WindowsBackgroundService<T>> logger)
        {
            (_service, _logger) = (service, logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(Shutdown);

            _logger.LogTrace("Starting service.");
            _service.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogDebug(ex, "Operation canceled; shutting down.");
                    break;
                }
            }
        }

        private void Shutdown()
        {
            _logger.LogTrace("Shutting down service via dispose.");
            _service?.Dispose();
        }
    }
}
