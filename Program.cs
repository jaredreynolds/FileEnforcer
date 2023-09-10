using FileEnforcer.Configuration;
using FileEnforcer.Services;

using IHost host = Host
    .CreateDefaultBuilder(args)
    .UseWindowsService(options => { options.ServiceName = "File Enforcer"; })
    .ConfigureServices((context, services) =>
    {
        services
            .AddMemoryCache()
            .Configure<FileEnforcementOptions>(context.Configuration.GetSection("Settings"))
            .AddSingleton<FileWatcherService>()
            .AddSingleton<FileEnforcementService>()
            .AddHostedService<DisposableBackgroundService<FileWatcherService>>();
    })
    .Build();

await host.RunAsync();
