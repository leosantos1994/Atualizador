namespace Updater.Models
{
    public class ClientUser : ModelBase
    {
        public Guid ClientId { get; set; }
        public virtual Client Client { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

    }
}
