using System.Linq.Expressions;
using Updater.Repository.Interfaces;

namespace Updater.Repository
{
    public class ClientRepository : IClientRepository
    {
        private AppDBContext _contextDB;
        public ClientRepository(AppDBContext context)
        {
            this._contextDB = context;
            _contextDB.Database.EnsureCreated();
        }

        public Models.Client Get(Guid Id)
        {
            return _contextDB.Client.FirstOrDefault(x => x.Id.Equals(Id));
        }

        public IEnumerable<Models.Client> GetAll()
        {
            return _contextDB.Client.AsEnumerable();
        }

        public IEnumerable<Models.Client> GetAll(Expression<Func<Models.Client, bool>> predicate)
        {
            return _contextDB.Client.Where(predicate).AsEnumerable();
        }

        public bool Any(Expression<Func<Models.Client, bool>> predicate)
        {
            return _contextDB.Client.Any(predicate);
        }

        public void Insert(Models.Client model)
        {
            _contextDB.Client.Add(model);
            _contextDB.SaveChanges();
        }

        public void Update(Models.Client model)
        {
            _contextDB.Client.Update(model);
            _contextDB.SaveChanges();
        }
    }
}
