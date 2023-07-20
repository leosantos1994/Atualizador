using System.Linq.Expressions;

namespace Updater.Repository.Interfaces
{
    public interface IVersionRepository
    {
        Models.Version Get(Guid Id);
        void Insert(Models.Version model, bool saveChanges = true);
        void Delete(Models.Version model, bool saveChanges = true);
        void Update(Models.Version model, bool saveChanges = true);
        void SaveChanges();
        IEnumerable<Models.Version> GetAll();
        IEnumerable<Models.Version> GetAll(Expression<Func<Models.Version, bool>> predicate);
    }
}
