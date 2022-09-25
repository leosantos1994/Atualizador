using Updater.Models;

namespace Updater.Repository.Interfaces
{
    public interface IVersionFileRepository
    {
        VersionFile Get(Guid Id);
        VersionFile GetByVersion(Guid versionId);
        void Insert(VersionFile model);
        void Update(VersionFile model);
    }
}
