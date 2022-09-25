namespace Updater.Models
{
    public class User : ModelBase
    {
        public string Username { get; set; } 
        public string Password { get; set; }
        public string Role { get; set; } 
        public string? Email { get; set; }
        public bool Locked { get; set; }
        public Guid? ClientId { get; set; }
        public Client Client { get; set; }


        public static implicit operator User(UserViewModel userViewModel)
        {
            return new User
            {
                Id = userViewModel.Id,
                Email = userViewModel.Email,
                Username = userViewModel.Username,
                Password = userViewModel.Password,
                Role = userViewModel.Role,
                Locked = userViewModel.Locked,
            };
        }

        public string IsLocked()
        {
            return this.Locked ? "Sim" : "Não";
        }

        public UserViewModel GetUserViewModel(User user)
        {
            return user;
        }
    }
}
