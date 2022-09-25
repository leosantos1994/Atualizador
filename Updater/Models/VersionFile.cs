namespace Updater.Models
{
    public class VersionFile : ModelBase
    {
        public VersionFile()
        {

        }
        public VersionFile(byte[] file, Guid version, string fileName)
        {
            File = file;
            VersionId = version;
            FileName = fileName;
        }

        public byte[] File { get; set; }
        public string FileName { get; set; }
        public Guid VersionId { get; set; }
        public Version Version { get; set; }
    }
}
