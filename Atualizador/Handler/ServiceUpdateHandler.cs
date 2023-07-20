using System.IO.Compression;
using System.Management;
using System.ServiceProcess;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public class ServiceUpdateHandler : BaseUpdateHandler
    {
        public ServiceUpdateHandler(ConfigSettings config) : base(config)
        {
        }

        private bool IsValid(out string path)
        {
            path = "";
            try
            {
                if (string.IsNullOrEmpty(ServiceModelHandler._service.ServiceName))
                    throw new Exception("Nome do serviço não informado.");

                Console.WriteLine($"Iniciando validações do serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");

                ResponseService.Add($"Iniciando validações do serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");

                ManagementObjectSearcher searcher = new("SELECT * FROM Win32_Service");
                ManagementObjectCollection collection = searcher.Get();

                Console.WriteLine("Varrendo os serviços");
                foreach (ManagementObject obj in collection)
                {
                    if (!Convert.ToString(obj[propertyName: "Name"]).Equals(ServiceModelHandler._service.ServiceName))
                        continue;

                    path = obj[propertyName: "PathName"].ToString().Substring(1).TrimEnd('\"');
                }

                Console.WriteLine("Caminho do serviço completo localizado " + path);
                if (File.Exists(path))
                {
                    path = new FileInfo(path).DirectoryName;

                    Console.WriteLine("Caminho do serviço localizado " + path);

                    return true;
                }

                else throw new DirectoryNotFoundException("Caminho do serviço não localizado ou não acessível.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Falha ao localizar serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow} Detalhe: {e.Message}");

                ResponseService.Add($"Falha ao localizar serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow} Detalhe: {e.Message}");
            }
            return false;
        }

        public void Service(string serviceName, bool start = true)
        {
            var serviceController = new ServiceController(serviceName);
            TimeSpan timeout = TimeSpan.FromMilliseconds(1000);

            if (start)
            {
                Console.WriteLine($"Iniciando serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
                ResponseService.Add($"Iniciando serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");

                if (serviceController.Status != ServiceControllerStatus.Running)
                {
                    serviceController.Start();

                    //serviceController.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }

                Console.WriteLine($"Serviço iniciado. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
                ResponseService.Add($"Serviço iniciado. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
            }
            else
            {
                Console.WriteLine($"Parando serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
                ResponseService.Add($"Parando serviço. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");

                if (serviceController.Status != ServiceControllerStatus.Stopped)
                {
                    serviceController.Stop();
                    //serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }

                Console.WriteLine($"Serviço parado. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
                ResponseService.Add($"Serviço parado. {ServiceModelHandler._service.ServiceName} em: {DateTime.UtcNow}");
            }

        }

        public void Init()
        {
            bool isValid = IsValid(out string rootFolder);

            if (!isValid)
            {
                Console.WriteLine($"Configurações para atualização inválidas. {ServiceModelHandler._service.ServiceName}"); return;
            }

            Service(ServiceModelHandler._service.ServiceName, false);

            Backup(ServiceModelHandler._service.ServiceName, rootFolder);

            ServiceModelHandler.ServiceFolderPath = rootFolder;

            string rootUpVersionFolder = ServiceModelHandler._service.PatchFilesPath.Replace(".zip", "");

            ZipFile.ExtractToDirectory(ServiceModelHandler._service.PatchFilesPath, rootUpVersionFolder, true);

            rootUpVersionFolder = Path.Combine(rootUpVersionFolder, "bin");

            Console.WriteLine("Caminho dos binários " + rootUpVersionFolder);

            if (!Path.Exists(rootUpVersionFolder))
            {
                throw new Exception("Binários não localizados, atualização dos serviços não concluída.");
            }

            UpdateService(rootFolder, rootUpVersionFolder);

            Service(ServiceModelHandler._service.ServiceName, true);

            Console.WriteLine("Serviço atualizado com sucesso.");
        }
    }
}
