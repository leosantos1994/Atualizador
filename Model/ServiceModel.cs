using System.ComponentModel.DataAnnotations;

namespace MidModel
{
    public class ServiceModel
    {
        [Key]
        public Guid Id { get; set; }
        public Guid VersionId { get; set; }
        public bool IsService { get; set; }
        public bool IsPool { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string PoolName { get; set; } = string.Empty;
        public string SiteUser { get; set; } = string.Empty;
        public string SitePass { get; set; } = string.Empty;
        public string PatchFilesPath { get; set; } = string.Empty;
        public bool HasUpdate { get; set; } = false;
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string VersionName { get; set; } = string.Empty;
        public Guid VersionFileId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public ScheduleProgress ScheduleProgress { get; set; } = ScheduleProgress.Waiting;

        public string GetBinariesPath(string prefix)
        {
            return Path.Combine(prefix, "bin");
        }

        public string GetReleasePath(string prefix)
        {
            return Path.Combine(prefix, "Release", "Script.xml");
        }
    }


    public enum ScheduleProgress
    {
        [Display(Name = "Aguardando")]
        Waiting,
        [Display(Name = "Iniciado")]
        Started,
        [Display(Name = "Concluído")]
        Done,
        [Display(Name = "Erro")]
        Error
    }
}