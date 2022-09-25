using System.Linq.Expressions;
using Updater.Models;

namespace Updater.Repository.Interfaces
{
    public interface IVersionRepository
    {
        Models.Version Get(Guid Id);
        void Insert(Models.Version model);
        void Update(Models.Version model);
        IEnumerable<Models.Version> GetAll();
        IEnumerable<Models.Version> GetAll(Expression<Func<Models.Version, bool>> predicate);
    }
}
