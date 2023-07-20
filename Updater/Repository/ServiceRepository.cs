using MidModel;
using System.Linq.Expressions;
using Updater.Repository.Interfaces;

namespace Updater.Repository
{
    public class ServiceRepository : IServiceRepository
    {
        private AppDBContext _contextDB;
        public ServiceRepository(AppDBContext context)
        {
            this._contextDB = context;
            _contextDB.Database.EnsureCreated();
        }

        public ServiceModel Get(Guid Id)
        {
            return _contextDB.Service.Where(x => x.Id == Id).FirstOrDefault();
        }

        public ServiceModel Get(Expression<Func<ServiceModel, bool>> predicate)
        {
            return _contextDB.Service.Where(predicate).FirstOrDefault();
        }

        public ServiceModel Get(string clientName)
        {
            return _contextDB.Service.Where(x => x.ClientName == clientName).FirstOrDefault();
        }
        public IEnumerable<ServiceModel> GetAll()
        {
            return _contextDB.Service;
        }

        public IEnumerable<ServiceModel> GetAll(Expression<Func<ServiceModel, bool>> predicate)
        {
            return _contextDB.Service.Where(predicate).AsEnumerable();
        }

        public void Insert(ServiceModel model)
        {
            _contextDB.Service.Add(model);
            _contextDB.SaveChanges();
        }

        public void Delete(ServiceModel model)
        {
            _contextDB.Service.Remove(model);
            _contextDB.SaveChanges();
        }

        public void Update(ServiceModel model)
        {
            _contextDB.ChangeTracker.Clear();
            _contextDB.Service.Update(model);
            _contextDB.SaveChanges();
        }
    }
}