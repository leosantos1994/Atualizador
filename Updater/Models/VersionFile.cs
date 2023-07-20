using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column(TypeName = "varbinary(max)")]
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public Guid VersionId { get; set; }
        public virtual Version Version { get; set; }
    }
}
