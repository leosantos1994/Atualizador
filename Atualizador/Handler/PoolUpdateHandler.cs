using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Web.Administration;
using Serilog;
using System.IO.Compression;
using System.Text.Json;
using System.Xml.Serialization;
using UpdaterService.Interfaces;
using UpdaterService.Model;
using Application = Microsoft.Web.Administration.Application;
using File = System.IO.File;

namespace UpdaterService.Handler
{
    public class PoolUpdateHandler : BaseUpdateHandler
    {
        public PoolUpdateHandler(IConfigSettings config) : base(config) { }
        private bool LoadSiteConfiguration(out PoolConfigModel poolConfig)
        {
            string PoolName = ServiceModelHandler._service.PoolName;

            if (string.IsNullOrEmpty(PoolName))
            {
                throw new Exception("Pool do site não informado.");
            }

            APIResponseHandler.Add($"Iniciando validações do site. {PoolName} em: {DateTime.UtcNow}");

            Site site = null;
            string url = "";
            Microsoft.Web.Administration.Application? application = null;

            site = FindIISSite(PoolName);

            if (site != null)
            {
                application = site.Applications.First();

                url = GetIISApplicationUrl(site);
            }
            else
            {
                (site, application) = FindIISApp(PoolName);

                if (site is null || application is null)
                {
                    throw new Exception("Site não localizado");
                }


                url = GetIISApplicationUrl(site, application);
            }

            if (application is null)
            {
                throw new Exception("Site não localizado");
            }


            Log.Information(" Pool name " + PoolName);
            Log.Information(" Pool directory " + application.VirtualDirectories.FirstOrDefault().PhysicalPath);
            Log.Information(" Pool url " + url);
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

        private string GetIISApplicationUrl(Site site, Application application)
        {
            string url = "";
            bool isHTTPS = false;

            if (application != null)
            {
                isHTTPS = application.EnabledProtocols.Contains("https") || site.Bindings.Any(x => x["Protocol"] as string == "https");

                if (isHTTPS)
                {
                    url = site.Bindings.First(x => x["Protocol"] as string == "https").BindingInformation + application.Path;
                    url = (isHTTPS ? "https://" : "http://") + url.Substring(url.LastIndexOf(":") + 1);
                    return url;
                }
                else
                    url = site.Bindings.First(x => x["Protocol"] as string == "http").BindingInformation + application.Path;

                string newUrl = (isHTTPS ? "https://" : "http://") + (url.Contains("*:80:") ? url.Replace("*:80:", "localhost") : "");

                url = newUrl;
            }
            else
            {
                isHTTPS = site.Bindings.Any(x => x["Protocol"] as string == "https");
                if (isHTTPS)
                    url = site.Bindings.First(x => x["Protocol"] as string == "https").BindingInformation + application.Path;
                else
                    url = site.Bindings.First(x => x["Protocol"] as string == "http").BindingInformation + application.Path;

                url = (isHTTPS ? "https://" : "http://") + url.Substring(url.LastIndexOf(":") + 1);
            }

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

        public void Pool(string poolName, bool start = true, bool recycle = false)
        {
            APIResponseHandler.Add($"Procurando pool de aplicativos. {ServiceModelHandler._service.PoolName}");

            var serverManager = new ServerManager();
            serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.ToLower().Equals(poolName.ToLower()));
            var appPool = serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.ToLower().Equals(poolName.ToLower()));

            if (recycle)
            {
                APIResponseHandler.Add($"Reciclando pool de aplicativos. {ServiceModelHandler._service.PoolName}");
                if (appPool?.State == ObjectState.Started)
                    appPool?.Recycle();
                APIResponseHandler.Add($"Pool de aplicativos reciclando. {ServiceModelHandler._service.PoolName}");
            }
            else if (start)
            {
                APIResponseHandler.Add($"Iniciando pool de aplicativos. {ServiceModelHandler._service.PoolName}");
                if (appPool?.State != ObjectState.Started)
                    appPool?.Start();
                APIResponseHandler.Add($"Pool de aplicativos iniciado. {ServiceModelHandler._service.PoolName}");
            }
            else
            {
                APIResponseHandler.Add($"Parando pool de aplicativos. {ServiceModelHandler._service.PoolName}");
                if (appPool?.State == ObjectState.Started)
                    appPool?.Stop();
                APIResponseHandler.Add($"Pool de aplicativos parado. {ServiceModelHandler._service.PoolName}");
            }
        }

        private bool ReleaseUpdate(string installerFullPath, string configPath)
        {
            var exec = new InstallerExeHandler(installerFullPath, new string[] { configPath });

            if (string.IsNullOrEmpty(exec.CommandErrors))
            {
                APIResponseHandler.Add($"Release de base atualizado com sucesso.");
                Log.Information("Release de base atualizado com sucesso.");
                return true;
            }
            else if (!string.IsNullOrEmpty(exec.CommandErrors))
            {
                APIResponseHandler.Add($"Release de base não atualizado com sucesso, verifique os dados ou tente atualizar manualmente. {exec.CommandErrors}");
                Log.Error($"Release de base não atualizado com sucesso, verifique os dados ou tente atualizar manualmente. {exec.CommandErrors}");
                return false;
            }
            return false;
        }


        private string CreateConfig(string site, string releasePath)
        {
            var file = new InstallerConfigModel().GetConfigFile(site, ServiceModelHandler._service.SiteUser, ServiceModelHandler._service.SitePass, releasePath);

            if (!Directory.Exists(Path.Combine(config.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName)))
                Directory.CreateDirectory(Path.Combine(config.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName));

            string path = Path.Combine(config.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName, "config.xml");
            var serializedConfg = new XmlSerializer(file.GetType());
            using (var writer = new StreamWriter(path))
            {
                serializedConfg.Serialize(writer, file);
            }
            return path;
        }

        public void Init()
        {
            Log.Information("\n Procurando configurações de IIS");

            bool isValid = LoadSiteConfiguration(out PoolConfigModel poolConfig);

            if (!isValid)
            {
                Log.Error($"Configurações para atualização inválidas. {ServiceModelHandler._service.PoolName}");
                APIResponseHandler.Add($"Configurações para atualização inválidas. {ServiceModelHandler._service.PoolName}"); return;
            }

            UpdateDBRelease(poolConfig);

            Log.Information("\n Parando pool");

            Pool(poolConfig.PoolName, start: false);

            Log.Information("\n Fazendo Backup");

            Backup(ServiceModelHandler._service.PoolName, poolConfig.RootPath);

            Log.Information("\n Atualizando");

            Update(poolConfig.RootPath, ServiceModelHandler._service.PatchFilesPath);

            ServiceModelHandler.SiteFolderPath = poolConfig.RootPath;

            Log.Information("\n Reiniciando pool");

            Pool(poolConfig.PoolName, start: true);

            APIResponseHandler.Add($"Procurando Release. {ServiceModelHandler._service.GetReleasePath(poolConfig.RootPath)}");

            Log.Information("\n Ambiente atualizado");

            APIResponseHandler.Add($"Ambiente atualizado. {ServiceModelHandler._service.PoolName}", complete: true);
            Log.Information($"Ambiente atualizado. {ServiceModelHandler._service.PoolName}");
        }

        private void UpdateDBRelease(PoolConfigModel poolConfig)
        {
            string releasePath = ExtractReleaseFile();

            Log.Information($"\n Procurando Release {releasePath}");

            if (File.Exists(releasePath))
            {
                APIResponseHandler.Add($"Release Localizado. {releasePath}");

                string configPath = CreateConfig(poolConfig.URL, releasePath);

                Log.Information("\n Aplicando atualização de base de dados (Release)");

                bool dbUpdateSucess = ReleaseUpdate(config.InstallerExePath, configPath);

                File.Delete(releasePath);

                if (!dbUpdateSucess)
                    throw new Exception("Erro ao atualizar banco de dados, verifique o log");

                Log.Information("\n Reiniciando pool");

                Pool(poolConfig.PoolName, start: false, recycle: true);
            }
            else
            {
                APIResponseHandler.Add($"Release Não Localizado. {releasePath}");
                Log.Information($"Release Não Localizado. {releasePath}");
            }
        }

        private string ExtractReleaseFile()
        {
            Log.Information($"Extraindo arquivo de release {ServiceModelHandler._service.PatchFilesPath}");
            using (var zip = ZipFile.OpenRead(ServiceModelHandler._service.PatchFilesPath))
            {
                var entry = zip.GetEntry("Release/Release.xml");
                if(entry != null)
                {
                    string destinationFile = Path.Combine(config.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName, "Release.xml");

                    if (!File.Exists(destinationFile))
                    {
                        File.Create(destinationFile).Dispose();
                    }

                    entry.ExtractToFile(destinationFile, true);
                    Log.Information($"Extração concluída {destinationFile}");
                    return destinationFile;
                }
            }

            return "";
        }
    }
}
