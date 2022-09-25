using System.ComponentModel.DataAnnotations;

namespace Updater.Models
{
    public class ModelBase : ICloneable
    {
        [Key]
        public Guid Id { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
