using System;
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
        }

        public VersionFile Get(Guid Id)
        {
            return _contextDB.VersionFile.FirstOrDefault(x => x.Id.Equals(Id));
        }

        public VersionFile GetByVersion(Guid versionId)
        {
            return _contextDB.VersionFile.Select(x => new VersionFile() { FileName = x.FileName, Id = x.Id, VersionId = x.VersionId }).FirstOrDefault(x => x.VersionId.Equals(versionId));
        }

        public string GetFileName(Guid versionId)
        {
            return _contextDB.VersionFile.Where(x => x.VersionId == versionId).Select(x => new VersionFile() { FileName = x.FileName, VersionId = x.Id }).FirstOrDefault().FileName;
        }

        public Guid GetIdByVersion(Guid versionId)
        {
            return _contextDB.VersionFile.Where(x => x.VersionId.Equals(versionId)).Select(x => x.Id).FirstOrDefault();
        }

        public void Insert(VersionFile model)
        {
            _contextDB.VersionFile.Add(model);
            _contextDB.SaveChanges();
        }

        public void Delete(VersionFile model)
        {
            _contextDB.VersionFile.Remove(model);
            _contextDB.SaveChanges();
        }

        public void Update(VersionFile model)
        {
            _contextDB.ChangeTracker.Clear();
            _contextDB.VersionFile.Update(model);
            _contextDB.SaveChanges();
        }
    }
}
