using System.ComponentModel.DataAnnotations;

namespace MidModel
{
    public class ServiceModel
    {
        [Key]
        public Guid Id { get; set; }
        public bool IsService { get; set; }
        public bool IsPool { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SiteUser { get; set; } = string.Empty;
        public string SitePass { get; set; } = string.Empty;
        public string PatchFilesPath { get; set; } = string.Empty;
        public string ReleaseFilePath { get; set; } = string.Empty;
        public bool HasUpdate { get; set; } = false;
    }
}