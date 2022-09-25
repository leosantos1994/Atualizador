using System.ComponentModel.DataAnnotations;

namespace Updater.Models
{
    public class Enums
    {
        public enum UserRole
        {
            [Display(Name = "Cliente")]
            client,
            [Display(Name = "Administrador")]
            sysadm
        }
    }
}
