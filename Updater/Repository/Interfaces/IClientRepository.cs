using System.Linq.Expressions;

namespace Updater.Repository.Interfaces
{
    public interface IClientRepository
    {
        Models.Client Get(Guid Id);
        void Insert(Models.Client model);
        void Update(Models.Client model);
        void Delete(Models.Client model);
        IEnumerable<Models.Client> GetAll();
        IEnumerable<Models.Client> GetAll(Expression<Func<Models.Client, bool>> predicate);
        bool Any(Expression<Func<Models.Client, bool>> predicate);
        void SaveChanges();
    }
}
