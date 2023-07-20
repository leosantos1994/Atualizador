using MidModel;
using System.Linq.Expressions;

namespace Updater.Repository.Interfaces
{
    public interface IServiceRepository
    {
        ServiceModel Get(Guid Id);
        ServiceModel Get(string clientName);
        IEnumerable<ServiceModel> GetAll();
        IEnumerable<ServiceModel> GetAll(Expression<Func<ServiceModel, bool>> predicate);
        ServiceModel Get(Expression<Func<ServiceModel, bool>> predicate);
        void Insert(ServiceModel model);
        void Update(ServiceModel model);
        void Delete(ServiceModel model);
    }
}
