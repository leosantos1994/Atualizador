using Serilog;
using System.IO.Compression;
using System.Management;
using System.ServiceProcess;
using UpdaterService.Interfaces;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public class ServiceUpdateHandler : BaseUpdateHandler
    {
        public ServiceUpdateHandler(IConfigSettings config) : base(config) { }

        private bool IsValid(out string path)
        {
            path = "";
            try
            {
                if (string.IsNullOrEmpty(ServiceModelHandler._service.ServiceName))
                {
                    Log.Error("Nome do serviço não informado.");
                    throw new Exception("Nome do serviço não informado.");
                }

                Log.Information($"Iniciando validações do serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");

                APIResponseHandler.Add($"Iniciando validações do serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");

                ManagementObjectSearcher searcher = new("SELECT * FROM Win32_Service");
                ManagementObjectCollection collection = searcher.Get();

                Log.Error("Varrendo os serviços");
                foreach (ManagementObject obj in collection)
                {
                    if (!Convert.ToString(obj[propertyName: "Name"]).Equals(ServiceModelHandler._service.ServiceName))
                        continue;

                    path = obj[propertyName: "PathName"].ToString().Substring(1).TrimEnd('\"');
                }

                Log.Information("Caminho do serviço completo localizado " + path);
                if (File.Exists(path))
                {
                    path = new FileInfo(path).DirectoryName;

                    Log.Information("Caminho do serviço localizado " + path);

                    return true;
                }

                else
                {
                    Log.Error("Caminho do serviço não localizado ou não acessível.");
                    throw new DirectoryNotFoundException("Caminho do serviço não localizado ou não acessível.");
                }
            }
            catch (Exception e)
            {
                Log.Information($"Falha ao localizar serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow} Detalhe: {e.Message}");

                Log.Error($"Falha ao localizar serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow} Detalhe: {e.Message}");

                APIResponseHandler.Add($"Falha ao localizar serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow} Detalhe: {e.Message}");
            }
            return false;
        }

        public void Service(string serviceName, bool start = true)
        {
            var serviceController = new ServiceController(serviceName);
            TimeSpan timeout = TimeSpan.FromMilliseconds(1000);

            if (start)
            {
                Log.Information($"Iniciando serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
                APIResponseHandler.Add($"Iniciando serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");

                if (serviceController.Status != ServiceControllerStatus.Running)
                {
                    serviceController.Start();

                    //serviceController.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }

                Log.Information($"Serviço iniciado. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
                APIResponseHandler.Add($"Serviço iniciado. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
            }
            else
            {
                Log.Information($"Parando serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
                APIResponseHandler.Add($"Parando serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");

                if (serviceController.Status != ServiceControllerStatus.Stopped)
                {
                    serviceController.Stop();
                    //serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }

                Log.Information($"Serviço parado. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
                APIResponseHandler.Add($"Serviço parado. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
            }

        }

        public void Init()
        {
            bool isValid = IsValid(out string rootFolder);

            if (!isValid)
            {
                Log.Error($"Configurações para atualização inválidas. {ServiceModelHandler._service.ServiceName}");
                Log.Information($"Configurações para atualização inválidas. {ServiceModelHandler._service.ServiceName}"); return;
            }

            Service(ServiceModelHandler._service.ServiceName, false);

            Backup(ServiceModelHandler._service.ServiceName, rootFolder);

            ServiceModelHandler.ServiceFolderPath = rootFolder;

            string rootUpVersionFolder = ServiceModelHandler._service.PatchFilesPath.Replace(".zip", "");

            Log.Information("Extraindo arquivos de atualização de " + ServiceModelHandler._service.PatchFilesPath + " para " + rootUpVersionFolder);

            ZipFile.ExtractToDirectory(ServiceModelHandler._service.PatchFilesPath, rootUpVersionFolder, true);

            string binFolder = Path.Combine(rootUpVersionFolder, "bin");

            Log.Information("Caminho dos binários " + binFolder);

            if (!Path.Exists(binFolder))
            {
                throw new Exception("Binários não localizados, atualização dos serviços não concluída.");
            }

            UpdateService(rootUpVersionFolder, binFolder);

            Service(ServiceModelHandler._service.ServiceName, true);

            Log.Information("Serviço atualizado com sucesso.");
            Log.Information("Serviço atualizado com sucesso.");
        }
    }
}
