namespace UpdaterService.Interfaces
{
    public interface IConfigSettings
    {
        string ApiURL { get; set; }
        string BakupFolder { get; set; }
        string Clients { get; set; }
        string InstallerExePath { get; set; }
        string ServiceWorkDir { get; set; }
    }
}