using System.ComponentModel.DataAnnotations;

namespace Updater.Models
{
    public class ClientViewModel : ModelBase
    {
        [Required(ErrorMessage = "Servidor é obrigatório")]
        [Display(Name = "Servidor")]
        public string Server { get; set; }
        [Required(ErrorMessage = "Nome é obrigatório")]
        [Display(Name = "Nome")]
        public string Name { get; set; }
        [Display(Name = "Bloqueado")]
        public bool Locked { get; set; }
        [Required(ErrorMessage = "Usuário do site é obrigatório")]
        [Display(Name = "Usuário do site")]
        public string SiteUser { get; set; }
        [Required(ErrorMessage = "Senha do site é obrigatória")]
        [Display(Name = "Senha do site")]
        public string SitePass { get; set; }
        [Required(ErrorMessage = "Pool de aplicativo é obrigatória")]
        [Display(Name = "Pool de aplicativo")]
        public string AppPoolName { get; set; }
        [Required(ErrorMessage = "Nome do serviço é obrigatório")]
        [Display(Name = "Nome do serviço")]
        public string ServiceName { get; set; }

        public static implicit operator ClientViewModel(Client clientmodel)
        {
            return new ClientViewModel
            {
                Id = clientmodel.Id,
                Name = clientmodel.Name,
                Locked = clientmodel.Locked,
                Server = clientmodel.Server,
                SitePass = clientmodel.SitePass,
                SiteUser = clientmodel.SiteUser,
                ServiceName = clientmodel.ServiceName,
                AppPoolName = clientmodel.AppPoolName,
            };
        }

        public string IsLocked()
        {
            return this.Locked ? "Sim" : "Não";
        }

        public Client GetClientModel(ClientViewModel client)
        {
            return client;
        }
    }
}
