using Updater.Models;
using Updater.Repository.Interfaces;

namespace Updater.Repository
{
    public class VersionFileRepository : IVersionFileRepository
    {
        private AppDBContext _contextDB;
        public VersionFileRepository(AppDBContext context)
        {
            this._contextDB = context;
            _contextDB.Database.EnsureCreated();
        }

        public VersionFile Get(Guid Id)
        {
            return _contextDB.VersionFile.FirstOrDefault(x => x.Id.Equals(Id));
        }

        public VersionFile GetByVersion(Guid versionId)
        {
            return _contextDB.VersionFile.FirstOrDefault(x => x.VersionId.Equals(versionId));
        }

        public void Insert(VersionFile model)
        {
            _contextDB.VersionFile.Add(model);
            _contextDB.SaveChanges();
        }

        public void Update(VersionFile model)
        {
            _contextDB.VersionFile.Update(model);
            _contextDB.SaveChanges();
        }
    }
}
