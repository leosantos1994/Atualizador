using System.ComponentModel.DataAnnotations;

namespace Updater.Models
{
    public class UserViewModel : ModelBase
    {
        [Display(Name = "Usuário")]
        [Required(ErrorMessage = "Usuário é obrigatório")]
        public string? Username { get; set; } = string.Empty;
        [Display(Name = "Senha")]
        [Required(ErrorMessage = "Senha é obrigatória")]
        public string? Password { get; set; } = string.Empty;
        [Display(Name = "Função")]
        [Required(ErrorMessage = "Função é obrigatória")]
        public string? Role { get; set; } = string.Empty;
        [Required(ErrorMessage = "E-mail é obrigatório")]
        [Display(Name = "E-mail")]
        public string? Email { get; set; } = string.Empty;
        [Display(Name = "Bloqueado")]
        public bool Locked { get; set; }
        public DateTime Creation { get; set; }

        public Client Client { get; set; }


        public static implicit operator UserViewModel(User userModel)
        {
            return new UserViewModel
            {
                Id = userModel.Id,
                Email = userModel.Email,
                Username = userModel.Username,
                Password = userModel.Password,
                Role = userModel.Role,
                Locked = userModel.Locked,
                Creation = userModel.Creation,
                Client = userModel.Clients?.FirstOrDefault()?.Client,
            };
        }

        public string IsLocked()
        {
            return this.Locked ? "Sim" : "Não";
        }

        public User GetUserModel(UserViewModel user)
        {
            return user;
        }
    }
}
