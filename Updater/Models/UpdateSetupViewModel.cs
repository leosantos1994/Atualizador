using System.ComponentModel.DataAnnotations;

namespace Updater.Models
{
    public class UpdateSetupViewModel : ModelBase
    {
        public Guid VersionId { get; set; }
        public bool IsPool { get; set; } = true;
        public bool IsService { get; set; }
        [Display(Name = "Data do agendamento")]
        [Required(ErrorMessage = "Data do agendamento é obrigatória")]
        public DateTime ScheduledDate { get; set; } = DateTime.Today;
        public DateTime CreationDate { get; set; }
        public List<Version> Versions { get; set; }
        public Version Version { get; set; }
        public List<Client> Clients { get; set; }
        public Client Client { get; set; }

        public static implicit operator UpdateSetup(UpdateSetupViewModel updateModel)
        {
            return new UpdateSetup
            {
                Id = updateModel.Id,
                IsPool = updateModel.IsPool,
                IsService = updateModel.IsService,
                ScheduledDate = updateModel.ScheduledDate,
                CreationDate = updateModel.ScheduledDate
            };
        }
    }
}
