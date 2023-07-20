using Microsoft.Web.Administration;
using System.Text.Json;
using System.Xml.Serialization;
using UpdaterService.Model;
using Application = Microsoft.Web.Administration.Application;
using File = System.IO.File;

namespace UpdaterService.Handler
{
    public class PoolUpdateHandler : BaseUpdateHandler
    {
        public PoolUpdateHandler(ConfigSettings config) : base(config)
        {
        }

        //private bool IsValid(out PoolConfigModel poolConfig)
        //{
        //    poolConfig = new PoolConfigModel();
        //    bool https = false;
        //    try
        //    {
        //        if (string.IsNullOrEmpty(_Service.PoolName))
        //            throw new Exception("Nome do site/pool não informado.");

        //        ResponseService.Add($"Iniciando validações do site. {_Service.PoolName} em: {DateTime.UtcNow}");

        //        var serverManager = new ServerManager();
        //        if (serverManager.Sites.Any(x => x.Name.ToLower().Equals(_Service.PoolName.ToLower())))
        //        {
        //            var site = serverManager.Sites.First(x => x.Name.ToLower().Equals(_Service.PoolName.ToLower()));
        //            https = site.Bindings.Any(x => x["Protocol"] as string == "https");

        //            if (https)
        //                poolConfig.URL = site.Bindings.First(x => x["Protocol"] as string == "https").BindingInformation;
        //            else
        //                poolConfig.URL = site.Bindings.First(x => x["Protocol"] as string == "http").BindingInformation;

        //            poolConfig.URL = poolConfig.URL.Substring(poolConfig.URL.LastIndexOf(":") + 1);

        //            var app = site.Applications.First();
        //            poolConfig.PoolName = app.ApplicationPoolName;
        //            poolConfig.RootPath = app.VirtualDirectories.First().PhysicalPath;
        //            return true;
        //        }
        //        else
        //        {
        //            foreach (var site in serverManager.Sites)
        //            {
        //                if (site.Applications.Any(x => x.Path.ToLower().EndsWith(_Service.PoolName.ToLower())))
        //                {
        //                    var app = site.Applications.First(x => x.Path.ToLower().EndsWith(_Service.PoolName.ToLower()));

        //                    if (https)
        //                        poolConfig.URL = site.Bindings.First(x => x["Protocol"] as string == "https").BindingInformation + "/" + _Service.PoolName;
        //                    else
        //                        poolConfig.URL = site.Bindings.First(x => x["Protocol"] as string == "http").BindingInformation + "/" + _Service.PoolName;

        //                    poolConfig.PoolName = app.ApplicationPoolName;
        //                    poolConfig.RootPath = app.VirtualDirectories.First().PhysicalPath;
        //                    return true;

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ResponseService.Add($"Falha ao localizar site. {_Service.PoolName} em: {DateTime.UtcNow} Detalhe: {JsonSerializer.Serialize(e)}");
        //    }
        //    return false;
        //}

        private bool LoadSiteConfiguration(out PoolConfigModel poolConfig)
        {
            string PoolName = ServiceModelHandler._service.PoolName;

            if (string.IsNullOrEmpty(PoolName))
                throw new Exception("Pool do site não informado.");

            ResponseService.Add($"Iniciando validações do site. {PoolName} em: {DateTime.UtcNow}");

            Site site = null;
            string url = "";
            Microsoft.Web.Administration.Application? application = null;

            Console.WriteLine("finding site");

            site = FindIISSite(PoolName);
            Console.WriteLine("finding site result" + site != null);

            if (site != null)
            {
                application = site.Applications.First();

                Console.WriteLine("finding app by site " + application != null);


                url = GetIISApplicationUrl(site);
            }
            else
            {
                Console.WriteLine("finding app and site");

                (site, application) = FindIISApp(PoolName);

                Console.WriteLine("finding app and site " + site != null && application != null);

                if (site is null || application is null)
                    throw new Exception("Site não localizado");


                url = GetIISApplicationUrl(site, application);
            }

            if (application is null)
                throw new Exception("Site não localizado");

            Console.WriteLine(" Pool name " + PoolName);
            Console.WriteLine(" Pool directory " + application.VirtualDirectories.FirstOrDefault().PhysicalPath);
            Console.WriteLine(" Pool url " + url);
            poolConfig = new()
            {
                PoolName = PoolName,
                RootPath = application.VirtualDirectories.FirstOrDefault().PhysicalPath,
                URL = url
            };

            return poolConfig.RootPath != "" && poolConfig.URL != "";
        }

        private (Site?, Application?) FindIISApp(string poolName)
        {
            Microsoft.Web.Administration.Application? application = null;

            var serverManager = new ServerManager();

            foreach (var site in serverManager.Sites)
            {
                if (site.Applications.Any(x => x.ApplicationPoolName.Equals(poolName) && x.Path != "/"))
                {
                    application = site.Applications.First(x => x.ApplicationPoolName.Equals(poolName) && x.Path != "/");

                    return (site, application);
                }
            }
            return (null, null);
        }

        private Site FindIISSite(string poolName)
        {
            Site site = null;

            var serverManager = new ServerManager();
            if (serverManager.Sites.Any(x => x.Name.Equals(poolName)))
            {
                site = serverManager.Sites.First(x => x.Name.Equals(poolName));
            }

            return site;
        }

        private string GetIISApplicationUrl(Site site, Microsoft.Web.Administration.Application application)
        {
            string url = "";
            bool isHTTPS = site.Bindings.Any(x => x["Protocol"] as string == "https");

            if (isHTTPS)
                url = site.Bindings.First(x => x["Protocol"] as string == "https").BindingInformation + application.Path;
            else
                url = site.Bindings.First(x => x["Protocol"] as string == "http").BindingInformation + application.Path;


            url = (isHTTPS ? "https://" : "http://") + url.Substring(url.LastIndexOf(":") + 1);

            return url;
        }

        private string GetIISApplicationUrl(Site site)
        {
            bool isHTTPS = site.Bindings.Any(x => x["Protocol"] as string == "https");
            string url = "";

            if (isHTTPS)
                url = site.Bindings.First(x => x["Protocol"] as string == "https").BindingInformation;
            else
                url = site.Bindings.First(x => x["Protocol"] as string == "http").BindingInformation;
            url = isHTTPS ? "https://" : "http://" + url.Substring(url.LastIndexOf(":") + 1);

            url = url.Substring(url.LastIndexOf(":") + 1);

            return url;
        }

        private void Pool(string poolName, bool start = true, bool recycle = false)
        {
            ResponseService.Add($"Procurando pool de aplicativos. {ServiceModelHandler._service.PoolName}");

            var serverManager = new ServerManager();
            serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.ToLower().Equals(poolName.ToLower()));
            var appPool = serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.ToLower().Equals(poolName.ToLower()));

            if (recycle)
            {
                ResponseService.Add($"Reciclando pool de aplicativos. {ServiceModelHandler._service.PoolName}");
                if (appPool?.State == ObjectState.Started)
                    appPool?.Recycle();
                ResponseService.Add($"Pool de aplicativos reciclando. {ServiceModelHandler._service.PoolName}");
            }
            else if (start)
            {
                ResponseService.Add($"Iniciando pool de aplicativos. {ServiceModelHandler._service.PoolName}");
                if (appPool?.State != ObjectState.Started)
                    appPool?.Start();
                ResponseService.Add($"Pool de aplicativos iniciado. {ServiceModelHandler._service.PoolName}");
            }
            else
            {
                ResponseService.Add($"Parando pool de aplicativos. {ServiceModelHandler._service.PoolName}");
                if (appPool?.State == ObjectState.Started)
                    appPool?.Stop();
                ResponseService.Add($"Pool de aplicativos parado. {ServiceModelHandler._service.PoolName}");
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
            var file = new InstallerConfigModel().GetConfigFile(site, ServiceModelHandler._service.SiteUser, ServiceModelHandler._service.SitePass, releasePath);

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
            Console.WriteLine("\n Procurando configurações de IIS");

            bool isValid = LoadSiteConfiguration(out PoolConfigModel poolConfig);

            if (!isValid)
            {
                Console.WriteLine("\n Configurações para atualização inválidas");

                ResponseService.Add($"Configurações para atualização inválidas. {ServiceModelHandler._service.PoolName}"); return;
            }

            Console.WriteLine("\n Parando pool");

            Pool(poolConfig.PoolName, start: false);

            Console.WriteLine("\n Fazendo Backup");

            Backup(ServiceModelHandler._service.PoolName, poolConfig.RootPath);

            Console.WriteLine("\n Atualizando");

            Update(poolConfig.RootPath, ServiceModelHandler._service.PatchFilesPath);

            ServiceModelHandler.SiteFolderPath = poolConfig.RootPath;

            Console.WriteLine("\n Reiniciando pool");

            Pool(poolConfig.PoolName, start: true);

            ResponseService.Add($"Procurando Release. {ServiceModelHandler._service.GetReleasePath(poolConfig.RootPath)}");

            Console.WriteLine($"\n Procurando Release {ServiceModelHandler._service.GetReleasePath(poolConfig.RootPath)}");

            if (File.Exists(ServiceModelHandler._service.GetReleasePath(poolConfig.RootPath)))
            {
                ResponseService.Add($"Release Localizado. {ServiceModelHandler._service.GetReleasePath(poolConfig.RootPath)}");

                string configPath = CreateConfig(poolConfig.URL, ServiceModelHandler._service.GetReleasePath(poolConfig.RootPath));

                Console.WriteLine("\n Aplicando atualização de base de dados (Release)");

                ReleaseUpdate(config.InstallerExePath, configPath);

                File.Delete(ServiceModelHandler._service.GetReleasePath(poolConfig.RootPath));

                Console.WriteLine("\n Reiniciando pool");

                Pool(poolConfig.PoolName, start: false, recycle: true);
            }
            else
            {
                ResponseService.Add($"Release Não Localizado. {ServiceModelHandler._service.GetReleasePath(poolConfig.RootPath)}");
                Console.WriteLine($"\n Release Não Localizado");
            }

            Console.WriteLine("\n Ambiente atualizado");

            ResponseService.Add($"Ambiente atualizado. {ServiceModelHandler._service.PoolName}", complete: true);
        }
    }
}
