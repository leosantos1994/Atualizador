using System.Linq.Expressions;

namespace Updater.Repository.Interfaces
{
    public interface IClientUserRepository
    {
        Models.ClientUser Get(Guid Id);
        Models.ClientUser GetByClient(Guid clientId);
        void Insert(Models.ClientUser model, bool saveChanges);
        void Update(Models.ClientUser model, bool saveChanges);
        void Delete(Models.ClientUser model, bool saveChanges);
        IEnumerable<Models.ClientUser> GetAll();
        IEnumerable<Models.ClientUser> GetAll(Expression<Func<Models.ClientUser, bool>> predicate);
        bool Any(Expression<Func<Models.ClientUser, bool>> predicate);
        void SaveChanges();
    }
}
