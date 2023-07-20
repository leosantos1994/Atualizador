using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Updater.Repository.Interfaces;

namespace Updater.Repository
{
    public class ClientUserRepository : IClientUserRepository
    {
        private AppDBContext _contextDB;
        public ClientUserRepository(AppDBContext context)
        {
            this._contextDB = context;
            _contextDB.Database.EnsureCreated();
        }

        public Models.ClientUser Get(Guid Id)
        {
            return _contextDB.ClientUser.Include(x => x.Id).FirstOrDefault(x => x.Id.Equals(Id));
        }

        public Models.ClientUser GetByClient(Guid clientId)
        {
            return _contextDB.ClientUser.FirstOrDefault(x=> x.ClientId == clientId);
        }

        public IEnumerable<Models.ClientUser> GetAll()
        {
            return _contextDB.ClientUser.Include(x => x.User).AsEnumerable();
        }

        public IEnumerable<Models.ClientUser> GetAll(Expression<Func<Models.ClientUser, bool>> predicate)
        {
            return _contextDB.ClientUser.Include(x => x.Client).Include(x => x.User).Where(predicate).AsEnumerable();
        }

        public bool Any(Expression<Func<Models.ClientUser, bool>> predicate)
        {
            return _contextDB.ClientUser.Any(predicate);
        }

        public void Insert(Models.ClientUser model, bool saveChanges = false)
        {
            _contextDB.ClientUser.Add(model);

            if (saveChanges)
                SaveChanges();
        }

        public void Update(Models.ClientUser model, bool saveChanges = false)
        {
            _contextDB.ChangeTracker.Clear();

            _contextDB.ClientUser.Update(model);

            if (saveChanges)
                SaveChanges();
        }

        public void Delete(Models.ClientUser model, bool saveChanges = false)
        {
            _contextDB.ClientUser.Remove(model);

            if (saveChanges)
                SaveChanges();
        }

        public void SaveChanges()
        {
            _contextDB.SaveChanges();
        }
    }
}
