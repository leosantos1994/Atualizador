using System;

namespace Updater.Models
{
    public class Version : ModelBase
    {
        public string ProductVersion { get; set; }
        public string Patch { get; set; }
        public DateTime Date { get; set; }
        public bool Locked { get; set; }
        public virtual VersionFile VersionFile { get; set; }


        public static implicit operator Version(VersionViewModel versionviewmodel)
        {
            return new Version
            {
                Date = versionviewmodel.Date,
                Id = versionviewmodel.Id,
                Locked = versionviewmodel.Locked,
                ProductVersion = versionviewmodel.Version,
                Patch = versionviewmodel.Patch,
            };
        }

        public VersionViewModel GetVersionViewModel(Version version)
        {
            return version;
        }
    }
}
