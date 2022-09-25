using MidModel;
using System.Text;
using UpdaterService.Handler;
using UpdaterService.Model;

namespace UpdaterService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly List<string> APILog = new List<string>();
        ConfigSettings _configSettings = new ConfigSettings();

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            config.GetSection(ConfigSettings.Config).Bind(_configSettings);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    await Task.Delay(30000, stoppingToken);

                    _logger.LogInformation("Seeking update at: {time}", DateTimeOffset.Now);

                    var request = APIHandler.FindUpdateRequest(_configSettings);

                    if (request.HasUpdate)
                        ExecuteOperation(request);
                    ExecuteOperation(new MidModel.ServiceModel() { IsPool = true, Name = "BRC2", SiteUser = "brconselhos", SitePass = "a123", ReleaseFilePath = @"C:\Users\leo_l\OneDrive\Área de Trabalho\release_mainimpl.xml" });
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Error at: {time}, {error}", DateTimeOffset.Now, ex);
                }
            }
        }

        private void ExecuteOperation(MidModel.ServiceModel request)
        {
            APILog.Append($"Localizada atualização em: {DateTimeOffset.Now}");

            InitUpdate(request);

            var logThreadCancellationToken = new CancellationTokenSource();
            Thread logThread = new(() => LogListener(request.Id, logThreadCancellationToken.Token));

            Thread updateThread = new(() => InitUpdate(request));
            while (updateThread.IsAlive)
            {
                Thread.Sleep(5000);

                if (!ResponseService.HasMessage())
                {
                    logThreadCancellationToken.Cancel();
                }
            }
        }

        private void LogListener(Guid serviceId, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ResponseService.SendResponse(serviceId, _configSettings);
                Thread.Sleep(1000);
            }
        }

        private void InitUpdate(MidModel.ServiceModel request)
        {
            if (request.IsPool)
                new PoolUpdateHandler(request, _configSettings).Init();

            else if (request.IsService)
                new ServiceUpdateHandler(request, _configSettings).Init();
        }
    }
}