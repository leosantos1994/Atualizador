namespace Updater.Models
{
    public class Client : ModelBase
    {
        public string? Server { get; set; }
        public string Name { get; set; }
        public bool Locked { get; set; }
        public string SiteUser { get; set; }
        public string SitePass { get; set; }
        public string ServiceName { get; set; }
        public string AppPoolName { get; set; }
        public DateTime Creation { get; set; }

        public virtual ICollection<ClientUser> Users { get; set; }


        public static implicit operator Client(ClientViewModel clientviewmodel)
        {
            return new Client
            {
                Id = clientviewmodel.Id,
                Name = clientviewmodel.Name,
                Locked = clientviewmodel.Locked,
                Server = clientviewmodel.Server,
                SiteUser = clientviewmodel.SiteUser,
                SitePass = clientviewmodel.SitePass,
                AppPoolName = clientviewmodel.AppPoolName,
                ServiceName = clientviewmodel.ServiceName
            };
        }

        public ClientViewModel GetClientViewModel(Client client)
        {
            return client;
        }
    }
}
