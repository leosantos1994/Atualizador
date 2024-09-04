using UpdaterService.Interfaces;

namespace UpdaterService.Model
{
    public class ConfigSettings : IConfigSettings
    {
        public const string Config = "Config";
        public string ApiURL { get; set; } = "";
        public string BackupFolder { get; set; } = "";
        public string ServiceWorkDir { get; set; } = "";
        public string InstallerExePath { get; set; } = "";
        public string Clients { get; set; } = "";
        public string AplicationPath { get; set; } = ""; 
        public bool Backup { get; set; } = true; 
    }
}
