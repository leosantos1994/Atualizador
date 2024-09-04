using MidModel;
using Serilog;
using System.IO.Compression;
using UpdaterService.Handler;
using UpdaterService.Interfaces;

namespace UpdaterService
{
    public sealed class Operator : IOperator
    {
        IConfigSettings _configSettings;

        public Operator(IConfigSettings _configSettings)
        {
            this._configSettings = _configSettings;
        }

        public void Operate()
        {
            ServiceModel serviceModel = APIHandler.FindUpdateRequest(_configSettings, out string error);

            if (!string.IsNullOrEmpty(error))
            {
                Log.Information(error);
            }
            else if (serviceModel.HasUpdate)
            {
                Log.Information("\n Executando operação");
                ServiceModelHandler.Updater(serviceModel);
                ExecuteOperation();
            }
        }

        private void ExecuteOperation()
        {
            Log.Information($"Localizada atualização em: {DateTimeOffset.Now}");

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
                Log.Error("Error at: {time}, {error}", DateTimeOffset.Now, ex);
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
                Log.Error(ex, "Ocorreu um erro ao atualizar, backup dos arquivos foi iniciado");
                ZipFile.ExtractToDirectory(ServiceModelHandler.BackupZipFile, ServiceModelHandler.SiteFolderPath, true);
           
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
                Log.Error(ex, "Ocorreu um erro ao atualizar os serviços, backup dos arquivos foi iniciado");
                ZipFile.ExtractToDirectory(ServiceModelHandler.BackupZipFile, ServiceModelHandler.ServiceFolderPath, true);
       
                new ServiceUpdateHandler(_configSettings).Service(ServiceModelHandler._service.ServiceName, true);
                throw;
            }

            Log.Information("Limpando diretório de serviço");
            string servicePath = Path.Combine(_configSettings.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName);

            foreach (var directory in Directory.EnumerateDirectories(servicePath))
            {
                Directory.Delete(directory, true);
            }

            Log.Information("Diretório de serviço foi limpo");
        }
    }
}
