using System.Linq.Expressions;
using Updater.Repository.Interfaces;

namespace Updater.Repository
{
    public class VersionRepository : IVersionRepository
    {
        private AppDBContext _contextDB;
        public VersionRepository(AppDBContext context)
        {
            this._contextDB = context;
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

        public void Insert(Models.Version model, bool saveChanges = true)
        {
            _contextDB.Version.Add(model);

            if (saveChanges)
                SaveChanges();
        }

        public void Delete(Models.Version model, bool saveChanges = true)
        {
            _contextDB.Version.Remove(model);

            if (saveChanges)
                SaveChanges();
        }

        public void Update(Models.Version model, bool saveChanges = true)
        {
            _contextDB.ChangeTracker.Clear();
            _contextDB.Version.Update(model);

            if (saveChanges)
                SaveChanges();
        }

        public void SaveChanges()
        {
            _contextDB.SaveChanges();
        }
    }
}
