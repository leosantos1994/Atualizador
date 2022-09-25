using System.Linq.Expressions;
using Updater.Models;
using Updater.Repository.Interfaces;

namespace Updater.Repository
{
    public class VersionRepository : IVersionRepository
    {
        private AppDBContext _contextDB;
        public VersionRepository(AppDBContext context)
        {
            this._contextDB = context;
            _contextDB.Database.EnsureCreated();
        }

        public Models.Version Get(Guid Id)
        {
            return _contextDB.Version.FirstOrDefault(x => x.Id.Equals(Id));
        }

        public IEnumerable<Models.Version> GetAll()
        {
            return _contextDB.Version.AsEnumerable();
        }

        public IEnumerable<Models.Version> GetAll(Expression<Func<Models.Version, bool>> predicate)
        {
            return _contextDB.Version.Where(predicate).AsEnumerable();
        }

        public void Insert(Models.Version model)
        {
            _contextDB.Version.Add(model);
            _contextDB.SaveChanges();
        }

        public void Update(Models.Version model)
        {
            _contextDB.Version.Update(model);
            _contextDB.SaveChanges();
        }
    }
}
