using UpdaterService;

IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        //services.AddLogging(ILogger);
    })
    .Build();

await host.RunAsync();
