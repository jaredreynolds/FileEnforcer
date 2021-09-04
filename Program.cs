using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FileEnforcer;
using FileEnforcer.Configuration;

using IHost host = Host
    .CreateDefaultBuilder(args)
    .UseWindowsService(options => { options.ServiceName = "File Enforcer"; })
    .ConfigureServices((context, services) =>
    {
        services
            .AddMemoryCache()
            .Configure<FileEnforcementOptions>(context.Configuration.GetSection("Settings"))
            .AddSingleton<FileWatcherService>()
            .AddSingleton<FileEnforcement>()
            .AddHostedService<WindowsBackgroundService<FileWatcherService>>();
    })
    .Build();

await host.RunAsync();
