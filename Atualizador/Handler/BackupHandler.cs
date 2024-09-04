using Serilog;
using System.IO.Compression;
using UpdaterService.Interfaces;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public abstract class BaseUpdateHandler
    {
        internal IConfigSettings config;
        static readonly string[] _extensoesRemover = new string[] { ".cs", ".pdb", ".csproj", ".config", ".XML", ".xml", ".resx", ".user", ".aspx.cs", ".aspx.designer.cs" };
        static readonly string[] _diretoriosExcluir = new string[] { "Bandeiras", "tmp", "Images", "img", "BackOffice", "DataSets" };

        public BaseUpdateHandler(IConfigSettings _config)
        {
            config = _config;
        }

        internal void Backup(string backupName, string backupFrom)
        {
            if (!config.Backup)
            {
                APIResponseHandler.Add($"Backup marcado para não realizar, backup não será executado. em: {DateTime.UtcNow}.");
                Log.Information($"Backup marcado para não realizar, backup não será executado. em: {DateTime.UtcNow}.");
                return;
            }

            string backup = $"{backupName}-{Constants.Constants.BackUpFolderName}-{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";

            string pastaBkp = Path.Combine(config.BackupFolder, backup);

            try
            {
                if (!Directory.Exists(config.BackupFolder))
                    Directory.CreateDirectory(config.BackupFolder);

                APIResponseHandler.Add($"Realizando backup da pasta em {pastaBkp} em: {DateTime.UtcNow}.");
                Log.Information($"Realizando backup da pasta em {pastaBkp} em: {DateTime.UtcNow}.");

                CopyDirectoryBackUp(backupFrom, pastaBkp, true);

                Log.Information($"Iniciando compressão do arquvio da pasta {pastaBkp} em {pastaBkp + ".zip"} este processo pode demorar um pouco. em: {DateTime.UtcNow}.");

                ZipFile.CreateFromDirectory(pastaBkp, pastaBkp + ".zip", CompressionLevel.Fastest, false);

                ServiceModelHandler.BackupZipFile = pastaBkp + ".zip";

                Log.Information($"Excluindo pasta de backup {pastaBkp} em: {DateTime.UtcNow}.");

                Directory.Delete(pastaBkp, true);

                Log.Information($"Backup realizado com sucesso na pasta {pastaBkp} em: {DateTime.UtcNow}.");
                APIResponseHandler.Add($"Backup realizado com sucesso na pasta {pastaBkp} em: {DateTime.UtcNow}.");
            }
            catch (Exception e)
            {
                Log.Error($"Erro ao tentar criar backup em {pastaBkp} de {backupFrom} em: {DateTime.UtcNow}.  Detalhe: {e.Message}");

                APIResponseHandler.Add($"Erro ao tentar criar backup em {pastaBkp} de {backupFrom} em: {DateTime.UtcNow}.  Detalhe: {e.Message}");
            }
        }

        internal void Update(string siteFolder, string patchFolder)
        {

            APIResponseHandler.Add($"Iniciando atualização da pasta em {siteFolder} em: {DateTime.UtcNow}.");

            APIResponseHandler.Add($"Localizando e excluindo dll 'HBSisConselhos.dll' em: {DateTime.UtcNow}.");

            string dllPath = Directory.GetFiles(siteFolder, "*.*", SearchOption.AllDirectories)
                                      .FirstOrDefault(x => x.Contains("HBSisConselhos.dll"));

            if (!string.IsNullOrEmpty(dllPath))
                File.Delete(dllPath);

            APIResponseHandler.Add($"Atualizando dll's em: {DateTime.UtcNow}.");

            ZipFile.ExtractToDirectory(patchFolder, siteFolder, true);

            APIResponseHandler.Add($"Atualização da pasta concluída em: {DateTime.UtcNow}.");
        }

        internal void UpdateService(string patchFolder, string binFolder)
        {

            APIResponseHandler.Add($"Iniciando atualização da pasta em {binFolder} em: {DateTime.UtcNow}.");

            APIResponseHandler.Add($"Atualizando dll's em: {DateTime.UtcNow}.");

            Log.Information($"Iniciando atualização da pasta em {binFolder} em: {DateTime.UtcNow}.");

            string destinationZip = Path.Combine(patchFolder, "bin-Files.zip");

            ZipFile.CreateFromDirectory(binFolder, destinationZip);

            Log.Information($"Extraindo dados");

            ZipFile.ExtractToDirectory(destinationZip, binFolder, true);

            APIResponseHandler.Add($"Atualização da pasta concluída em: {DateTime.UtcNow}.");
        }

        private static void CopyAll(string siteFolder, string patchFolder)
        {
            var sourcePath = new DirectoryInfo(patchFolder);

            foreach (FileInfo fi in sourcePath.GetFiles())
            {
                Log.Information($"Tentando copiar arquivo " + Path.Combine(siteFolder, fi.Name));
                fi.CopyTo(Path.Combine(siteFolder, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in sourcePath.GetDirectories())
            {
                CopyAll(sourcePath.FullName, patchFolder);
            }
        }

        static void CopyDirectoryBackUp(string sourceDir, string destinationDir, bool recursive)
        {
            Log.Information($"Realizando backup da pasta {sourceDir} para a pasta {destinationDir} em: {DateTime.UtcNow}.");

            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (_diretoriosExcluir.Any(c => destinationDir.EndsWith(c)))
                return;

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                if (!_extensoesRemover.Contains(file.Extension))
                    file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectoryBackUp(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
