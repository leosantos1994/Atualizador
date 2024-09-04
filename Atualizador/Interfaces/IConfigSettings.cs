namespace UpdaterService.Interfaces
{
    public interface IConfigSettings
    {
        string ApiURL { get; set; }
        string BackupFolder { get; set; }
        string Clients { get; set; }
        string InstallerExePath { get; set; }
        string ServiceWorkDir { get; set; }
        string AplicationPath { get; set; }
        bool Backup { get; set; }
    }
}