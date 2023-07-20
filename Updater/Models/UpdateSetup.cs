using static Updater.Models.Enums;

namespace Updater.Models
{
    public class UpdateSetup : ModelBase
    {
        public Guid VersionId { get; set; }
        public Guid ClientId { get; set; }
        public bool IsPool { get; set; }
        public bool IsService { get; set; }
        public DateTime ScheduledDate { get; set; }

        public static implicit operator UpdateSetupViewModel(UpdateSetup updateModel)
        {
            return new UpdateSetupViewModel
            {
                Id = updateModel.Id,
                IsPool = updateModel.IsPool,
                IsService = updateModel.IsService,
                ScheduledDate = updateModel.ScheduledDate
            };
        }
    }
}
