using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Updater.Models;
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
            return _contextDB.Client.Include(x=> x.Users).FirstOrDefault(x => x.Id.Equals(Id));
        }

        public IEnumerable<Models.Client> GetAll()
        {
            return _contextDB.Client.Include(x=> x.Users).ThenInclude(x=> x.User).AsEnumerable();
        }

        public IEnumerable<Models.Client> GetAll(Expression<Func<Models.Client, bool>> predicate)
        {
            return _contextDB.Client.Include(x => x.Users).ThenInclude(x => x.User).Where(predicate).AsEnumerable();
        }

        public bool Any(Expression<Func<Models.Client, bool>> predicate)
        {
            return _contextDB.Client.Any(predicate);
        }

        public void Insert(Models.Client model)
        {
            model.Creation = DateTime.Now;
            _contextDB.Client.Add(model);
        }

        public void Update(Models.Client model)
        {
            _contextDB.ChangeTracker.Clear();

            _contextDB.Client.Update(model);
        }

        public void SaveChanges()
        {
            _contextDB.SaveChanges();
        }

        public void Delete(Client model)
        {
            _contextDB.Client.Remove(model);

        }
    }
}
