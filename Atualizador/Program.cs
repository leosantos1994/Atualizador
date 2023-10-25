using Serilog;
using UpdaterService;
using UpdaterService.Model;
using UpdaterService.Interfaces;
using Serilog.Events;
using System.Reflection;

IConfigSettings config = new ConfigSettings();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File("./logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();


Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
IHost host = Host.CreateDefaultBuilder(args)
       .UseWindowsService(options =>
       {
           options.ServiceName = "Atualizador";
       })
       .ConfigureAppConfiguration((hostContext, configBuilder) =>
       {
           configBuilder
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
               .Build();
       })
       .UseSerilog((context, services, configuration) => configuration
           .ReadFrom.Configuration(context.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext())
       .ConfigureServices((hostContext, services) =>
       {
           hostContext.Configuration.GetSection(ConfigSettings.Config).Bind(config);
           services.AddHostedService<Worker>();
           services.AddSingleton<IConfigSettings>(config);
           services.AddSingleton<IOperator, Operator>();
       })
       .Build();


await host.RunAsync();