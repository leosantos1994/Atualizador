using System.Linq.Expressions;

namespace Updater.Repository.Interfaces
{
    public interface IUserRepository
    {
        Models.User Get(Guid Id);
        void Insert(Models.User model);
        void Update(Models.User model);
        IEnumerable<Models.User> GetAll();
        IEnumerable<Models.User> GetAll(Expression<Func<Models.User, bool>> predicate);
        bool Any(Expression<Func<Models.User, bool>> predicate);
        string HashPass(string pass);
        string GeneratePrivateKey();
    }
}
