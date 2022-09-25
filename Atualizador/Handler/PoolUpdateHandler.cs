using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public class PoolUpdateHandler : BaseUpdateHandler
    {
        MidModel.ServiceModel service;

        public PoolUpdateHandler(MidModel.ServiceModel _service, ConfigSettings config) : base(config)
        {
            service = _service;
        }

        private bool IsValid(out PoolConfigModel poolConfig)
        {
            poolConfig = new PoolConfigModel();
            bool https = false;
            try
            {
                ResponseService.Add($"Iniciando validações do site. {service.Name} em: {DateTime.UtcNow}");

                var serverManager = new ServerManager();
                if (serverManager.Sites.Any(x => x.Name.ToLower().Equals(service.Name.ToLower())))
                {
                    var site = serverManager.Sites.First(x => x.Name.ToLower().Equals(service.Name.ToLower()));
                    https = site.Bindings.Any(x => x["Protocol"] as string == "https");

                    if (https)
                        poolConfig.URL = site.Bindings.First(x => x["Protocol"] as string == "https").BindingInformation;
                    else
                        poolConfig.URL = site.Bindings.First(x => x["Protocol"] as string == "http").BindingInformation;

                    poolConfig.URL = poolConfig.URL.Substring(poolConfig.URL.LastIndexOf(":") + 1);

                    var app = site.Applications.First();
                    poolConfig.PoolName = app.ApplicationPoolName;
                    poolConfig.RootPath = app.VirtualDirectories.First().PhysicalPath;
                    return true;
                }
                else
                {
                    foreach (var site in serverManager.Sites)
                    {
                        if (site.Applications.Any(x => x.Path.ToLower().EndsWith(service.Name.ToLower())))
                        {
                            var app = site.Applications.First(x => x.Path.ToLower().EndsWith(service.Name.ToLower()));

                            if (https)
                                poolConfig.URL = site.Bindings.First(x => x["Protocol"] as string == "https").BindingInformation + "/" + service.Name;
                            else
                                poolConfig.URL = site.Bindings.First(x => x["Protocol"] as string == "http").BindingInformation + "/" + service.Name;

                            poolConfig.PoolName = app.ApplicationPoolName;
                            poolConfig.RootPath = app.VirtualDirectories.First().PhysicalPath;
                            return true;

                        }
                    }
                }
            }
            catch (Exception e)
            {
                ResponseService.Add($"Falha ao localizar site. {service.Name} em: {DateTime.UtcNow} Detalhe: {JsonSerializer.Serialize(e)}");
            }
            return false;
        }

        private void Pool(string poolName, bool start = true, bool recycle = false)
        {
            ResponseService.Add($"Procurando pool de aplicativos. {service.Name}");

            var serverManager = new ServerManager();
            serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.ToLower().Equals(poolName.ToLower()));
            var appPool = serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.ToLower().Equals(poolName.ToLower()));

            if (recycle)
            {
                ResponseService.Add($"Reciclando pool de aplicativos. {service.Name}");
                if (appPool?.State == ObjectState.Started)
                    appPool?.Recycle();
                ResponseService.Add($"Pool de aplicativos reciclando. {service.Name}");
            }
            else if (start)
            {
                ResponseService.Add($"Iniciando pool de aplicativos. {service.Name}");
                if (appPool?.State != ObjectState.Started)
                    appPool?.Start();
                ResponseService.Add($"Pool de aplicativos iniciado. {service.Name}");
            }
            else
            {
                ResponseService.Add($"Parando pool de aplicativos. {service.Name}");
                if (appPool?.State == ObjectState.Started)
                    appPool?.Stop();
                ResponseService.Add($"Pool de aplicativos parado. {service.Name}");
            }
        }

        private void ReleaseUpdate(string installerFullPath, string configPath)
        {
            var exec = new InstallerExeHandler(installerFullPath, new string[] { configPath });

            if (string.IsNullOrEmpty(exec.CommandErrors))
                ResponseService.Add($"Release de base atualizado com sucesso.");

            else if (!string.IsNullOrEmpty(exec.CommandErrors))
                ResponseService.Add($"Release de base não atualizado com sucesso, verifique os dados ou tente atualizar manualmente. {exec.CommandErrors}");
        }

        private string CreateConfig(string site, string releasePath)
        {
            var file = new InstallerConfigModel().GetConfigFile(site, service.SiteUser, service.SitePass, releasePath);

            if (!Directory.Exists(config.ServiceWorkDir))
                Directory.CreateDirectory(config.ServiceWorkDir);

            string path = Path.Combine(config.ServiceWorkDir, "config.xml");
            var serializedConfg = new XmlSerializer(file.GetType());
            using (var writer = new StreamWriter(path))
            {
                serializedConfg.Serialize(writer, file);
            }
            return path;
        }

        public void Init()
        {
            string servicePath = Path.Combine(config.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName);

            bool isValid = IsValid(out PoolConfigModel poolConfig);

            if (!isValid)
            {
                ResponseService.Add($"Configurações para atualização inválidas. {service.Name}"); return;
            }

            Pool(poolConfig.PoolName, start: false);

            Backup(service.Name, poolConfig.RootPath);

            Update(poolConfig.RootPath, servicePath);

            Pool(poolConfig.PoolName, start: true);

            string configPath = CreateConfig(poolConfig.URL, service.ReleaseFilePath);

            ReleaseUpdate(config.InstallerExePath, configPath);

            Pool(poolConfig.PoolName, start: false, recycle: true);

            ResponseService.Add($"Ambiente atualizado. {service.Name}", complete: true);
        }
    }
}
