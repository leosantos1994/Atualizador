namespace UpdaterService.Model
{
    public class ConfigSettings
    {
        public const string Config = "Config";
        public string Client { get; set; } = "";
        public string ApiURL { get; set; } = "";
        public string BakupFolder { get; set; } = "";
        public string ServiceWorkDir { get; set; } = "";
        public string InstallerExePath { get; set; } = "";
    }
}
