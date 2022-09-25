using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public abstract class BaseUpdateHandler
    {
        internal ConfigSettings config;
        public BaseUpdateHandler(ConfigSettings _config)
        {
            config = _config;
        }

        internal void Backup(string backupName, string backupFrom)
        {
            string backup = $"{backupName}-{Constants.Constants.BackUpFolderName}-{DateTime.Now:dd-MM-yyyy-HH-mm-ss}.zip";

            string pastaBkp = Path.Combine(config.BakupFolder, backup);

            try
            {
                if (!Directory.Exists(config.BakupFolder))
                    Directory.CreateDirectory(config.BakupFolder);

                ResponseService.Add($"Realizando backup da pasta em {pastaBkp} em: {DateTime.UtcNow}.");

                ZipFile.CreateFromDirectory(backupFrom, pastaBkp);

                ResponseService.Add($"Backup realizado com sucesso na pasta {pastaBkp} em: {DateTime.UtcNow}.");
            }
            catch (Exception e)
            {
                ResponseService.Add($"Erro ao tentar criar backup em {pastaBkp} de {backupFrom} em: {DateTime.UtcNow}.  Detalhe: {JsonSerializer.Serialize(e)}");
            }
        }

        internal void Update(string rootFolder, string rootUpVerionFolder)
        {

            ResponseService.Add($"Iniciando atualização da pasta em {rootFolder} em: {DateTime.UtcNow}.");

            ResponseService.Add($"Localizando e excluindo dll 'HBSisConselhos.dll' em: {DateTime.UtcNow}.");

            string dllPath = Directory.GetFiles(rootFolder, "*.*", SearchOption.AllDirectories)
                                      .FirstOrDefault(x => x.Contains("HBSisConselhos.dll"));

            if (!string.IsNullOrEmpty(dllPath))
                File.Delete(dllPath);

            ResponseService.Add($"Atualizando dll's em: {DateTime.UtcNow}.");

            ZipFile.ExtractToDirectory(rootUpVerionFolder, rootFolder, true);

            ResponseService.Add($"Atualização da pasta concluída em: {DateTime.UtcNow}.");
        }
    }
}
