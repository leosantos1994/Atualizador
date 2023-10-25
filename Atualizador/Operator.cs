using MidModel;
using Serilog;
using System.IO.Compression;
using UpdaterService.Handler;
using UpdaterService.Interfaces;

namespace UpdaterService
{
    public sealed class Operator : IOperator
    {
        private readonly List<string> APILog = new List<string>();
        IConfigSettings _configSettings;

        public Operator(IConfigSettings _configSettings)
        {
            this._configSettings = _configSettings;
        }

        public void Operate()
        {
            var request = APIHandler.FindUpdateRequest(_configSettings, out string error);

            if (!string.IsNullOrEmpty(error))
            {
                Log.Information(error);
            }
            else if (request.HasUpdate)
            {
                Log.Information("\n Executando operação");
                ServiceModelHandler.Updater(request);
                ExecuteOperation();
            }
        }

        private void ExecuteOperation()
        {
            APILog.Append($"Localizada atualização em: {DateTimeOffset.Now}");
            var logThreadCancellationToken = new CancellationTokenSource();
            try
            {
                Log.Information("\n Atualizando status");

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
                Log.Information("Error at: {time}, {error}", DateTimeOffset.Now, ex);

                Log.Error("Ocorreu um erro", ex);
                Log.Information("Ocorreu um erro " + ex.Message);
                //logThreadCancellationToken.Cancel();
                APIHandler.SendStatusInformation(_configSettings, ServiceModelHandler._service.Id, (int)ScheduleProgress.Error);
                throw;
            }
        }

        private void LogListener(Guid serviceId, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                APIResponseHandler.SendResponse(serviceId, _configSettings);
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
                Log.Error(ex, "Ocorreu um erro ao atualizar o backup dos arquivos foi iniciado");
                ZipFile.ExtractToDirectory(ServiceModelHandler.BackupZipFile, ServiceModelHandler.SiteFolderPath, true);
                Log.Error(ex, "Ocorreu um erro ao atualizar o backup dos arquivos foi executado");
                Log.Information("Ocorreu um erro ao atualizar o backup dos arquivos foi executado");
                new PoolUpdateHandler(_configSettings).Pool(ServiceModelHandler._service.PoolName, true);
                throw;
            }

            try
            {
                if (ServiceModelHandler._service.IsService)
                    new ServiceUpdateHandler(_configSettings).Init();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ocorreu um erro ao atualizar os serviços o backup dos arquivos foi iniciado");
                ZipFile.ExtractToDirectory(ServiceModelHandler.BackupZipFile, ServiceModelHandler.ServiceFolderPath, true);
                Log.Error(ex, "Ocorreu um erro ao atualizar os serviços o backup dos arquivos foi executado");
                Log.Information("Ocorreu um erro ao atualizar o backup dos arquivos foi executado");
                new ServiceUpdateHandler(_configSettings).Service(ServiceModelHandler._service.ServiceName, true);
                throw;
            }
            Log.Error("Limpando diretório de serviço");

            string servicePath = Path.Combine(_configSettings.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName);

            foreach (var directory in Directory.EnumerateDirectories(servicePath))
            {
                Directory.Delete(directory, true);
            }

            Log.Error("Diretório de serviço foi limpo");
        }
    }
}
