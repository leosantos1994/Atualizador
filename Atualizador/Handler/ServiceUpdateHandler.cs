using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public class ServiceUpdateHandler : BaseUpdateHandler
    {
        MidModel.ServiceModel service;
        public ServiceUpdateHandler(MidModel.ServiceModel _service, ConfigSettings config) : base(config)
        {
            service = _service;
        }

        private bool IsValid(out string path)
        {
            path = "";
            try
            {
                ResponseService.Add($"Iniciando validações do serviço. {service.Name} em: {DateTime.UtcNow}");

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    if (!Convert.ToString(obj[propertyName: "Name"]).Equals(service.Name))
                        continue;
                    path = Path.GetDirectoryName(Convert.ToString(obj[propertyName: "PathName"]));
                }

                if (Directory.Exists(path)) return true;

                else throw new DirectoryNotFoundException("Caminho do serviço não localizado ou não acessível.");
            }
            catch (Exception e)
            {
                ResponseService.Add($"Falha ao localizar serviço. {service.Name} em: {DateTime.UtcNow} Detalhe: {JsonSerializer.Serialize(e)}");
            }
            return false;
        }

        public void Service(string serviceName, bool start = true)
        {
            var serviceController = new ServiceController(serviceName);
            TimeSpan timeout = TimeSpan.FromMilliseconds(1000);

            if (start)
            {
                ResponseService.Add($"Iniciando serviço. {service.Name} em: {DateTime.UtcNow}");
                serviceController.Start();
                ResponseService.Add($"Serviço iniciado. {service.Name} em: {DateTime.UtcNow}");
            }
            else
            {
                ResponseService.Add($"Parando serviço. {service.Name} em: {DateTime.UtcNow}");
                serviceController.Stop();
                ResponseService.Add($"Serviço parado. {service.Name} em: {DateTime.UtcNow}");
            }

            serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        }

        public void Init()
        {
            string rootUpVerionFolder = "";

            bool isValid = IsValid(out string rootFolder);

            if (!isValid)
            {
                ResponseService.Add($"Configurações para atualização inválidas. {service.Name}"); return;
            }

            Service(service.Name, false);

            Backup(service.Name, rootFolder);

            Update(rootFolder, rootUpVerionFolder);

            Service(service.Name, true);
        }
    }
}
