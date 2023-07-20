using Updater.Models;

namespace Updater.Repository.Interfaces
{
    public interface IVersionFileRepository
    {
        VersionFile Get(Guid Id);
        void Delete(VersionFile model);

        Guid GetIdByVersion(Guid versionId);
        string GetFileName(Guid versionId);
        VersionFile GetByVersion(Guid versionId);
        void Insert(VersionFile model);
        void Update(VersionFile model);
    }
}
