using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Updater.Models
{
    public class VersionViewModel : ModelBase
    {
        [Required(ErrorMessage = "Versão é obrigatória")]
        [Display(Name = "Versão")]
        public string Version { get; set; }
        [Required(ErrorMessage = "Patch é obrigatório")]
        [Display(Name = "Patch")]
        public string Patch { get; set; }
        [Required(ErrorMessage = "Data é obrigatória")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Data")]
        public DateTime Date { get; set; }
        [Display(Name = "Bloqueada")]
        public bool Locked { get; set; }
        [Required(ErrorMessage = "Arquivo é obrigatório")]
        [Display(Name = "Arquivos (.ZIP)")]
        public IFormFile File { get; set; }
        public Guid VersionFileId { get; set; }

        public static implicit operator VersionViewModel(Version versionmodel)
        {
            return new VersionViewModel
            {
                Date = versionmodel.Date,
                Id = versionmodel.Id,
                Locked = versionmodel.Locked,
                Version = versionmodel.ProductVersion,
                Patch = versionmodel.Patch,
                VersionFileId = versionmodel.VersionFile.Id,
            };
        }
        public string IsLocked()
        {
            return this.Locked ? "Sim" : "Não";
        }
        public Version GetVersionModel(VersionViewModel version)
        {
            return version;
        }
    }
}
