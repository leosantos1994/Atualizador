using System.Linq.Expressions;

namespace Updater.Repository.Interfaces
{
    public interface IUserClientRepository
    {
        Models.User Get(Guid Id);
        void Insert(Models.User model, bool saveChanges = true);
        void Update(Models.User model, bool saveChanges = true);
        IEnumerable<Models.User> GetAll();
        IEnumerable<Models.User> GetAll(Expression<Func<Models.User, bool>> predicate);
        bool Any(Expression<Func<Models.User, bool>> predicate);
        string HashPass(string pass);
        string GeneratePrivateKey();
    }
}
