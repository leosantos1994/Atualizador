using Microsoft.Web.Administration;
using MidModel;
using System.IO.Compression;
using System.Xml;
using UpdaterService.Handler;
using UpdaterService.Model;

namespace UpdaterService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly List<string> APILog = new List<string>();
        ConfigSettings _configSettings = new ConfigSettings();
        private static bool IsTaskCompleted = true;
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            config.GetSection(ConfigSettings.Config).Bind(_configSettings);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            while (IsTaskCompleted)
            {
                if (IsTaskCompleted)
                {
                    try
                    {

                        IsTaskCompleted = false;

                        _logger.LogInformation("Seeking update at: {time}", DateTimeOffset.Now);

                        var request = APIHandler.FindUpdateRequest(_configSettings, out string error);

                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine(error);
                            //_logger.LogInformation("Error at: {time}, {error}", DateTimeOffset.Now, error);
                        }
                        else if (request.HasUpdate)
                        {
                            Console.WriteLine("\n Executando operação");
                            ServiceModelHandler.Updater(request);
                            ExecuteOperation();
                        }
                    }
                    catch (Exception ex)
                    {
                        IsTaskCompleted = true;

                        _logger.LogInformation("Error at: {time}, {error}", DateTimeOffset.Now, ex);

                        Console.WriteLine(ex);
                    }

                    Thread.Sleep(10000);
                    IsTaskCompleted = true;
                }
            }
        }

        private void ExecuteOperation()
        {
            APILog.Append($"Localizada atualização em: {DateTimeOffset.Now}");
            var logThreadCancellationToken = new CancellationTokenSource();

            try
            {
                Console.WriteLine("\n Atualizando status");

                APIHandler.SendStatusInformation(_configSettings, ServiceModelHandler._service.Id, (int)ScheduleProgress.Started);

                InitUpdate();

                //Thread logThread = new(() => LogListener(request.Id, logThreadCancellationToken.Token));

                //Thread updateThread = new(() => InitUpdate(request));
                //while (logThread.IsAlive)
                //{
                //    Thread.Sleep(5000);

                //    if (!ResponseService.HasMessage())
                //    {
                //        logThreadCancellationToken.Cancel();
                //    }
                //}
                APIHandler.SendStatusInformation(_configSettings, ServiceModelHandler._service.Id, (int)ScheduleProgress.Done);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro " + ex.Message);
                //logThreadCancellationToken.Cancel();
                APIHandler.SendStatusInformation(_configSettings, ServiceModelHandler._service.Id, (int)ScheduleProgress.Error);
                throw;
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

        private void InitUpdate()
        {
            try
            {
                if (ServiceModelHandler._service.IsPool)
                    new PoolUpdateHandler(_configSettings).Init();
            }
            catch (Exception ex)
            {
                ZipFile.ExtractToDirectory(ServiceModelHandler.BackupZipFile, ServiceModelHandler.SiteFolderPath, true);
                _logger.LogError(ex, "Ocorreu um erro ao atualizar o backup dos arquivos foi executado");
                Console.WriteLine("Ocorreu um erro ao atualizar o backup dos arquivos foi executado");
                throw;
            }

            try
            {
                if (ServiceModelHandler._service.IsService)
                    new ServiceUpdateHandler(_configSettings).Init();
            }
            catch (Exception ex)
            {
                ZipFile.ExtractToDirectory(ServiceModelHandler.BackupZipFile, ServiceModelHandler.ServiceFolderPath, true);
                _logger.LogError(ex, "Ocorreu um erro ao atualizar os serviços o backup dos arquivos foi executado");
                Console.WriteLine("Ocorreu um erro ao atualizar o backup dos arquivos foi executado");
                throw;
            }
        }
    }
}