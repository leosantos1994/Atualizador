using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Updater.Repository.Interfaces;

namespace Updater.Repository
{
    public class UserRepository : IUserClientRepository
    {
        AppDBContext _contextDB;
        string AUTH_TOKEN = "a1jaWhyK08rE5e1mrC8U4P+SBGTOC1wK3mUKXOoaJ6s=";
        public UserRepository(AppDBContext context)
        {
            this._contextDB = context;
        }

        public Models.User Get(Guid Id)
        {
            return _contextDB.User.Include(x => x.Clients).FirstOrDefault(x => x.Id.Equals(Id));
        }

        public IEnumerable<Models.User> GetAll()
        {
            return _contextDB.User.AsEnumerable();
        }

        public IEnumerable<Models.User> GetAll(Expression<Func<Models.User, bool>> predicate)
        {
            return _contextDB.User.Include(x=> x.Clients).Where(predicate).AsEnumerable();
        }

        public bool Any(Expression<Func<Models.User, bool>> predicate)
        {
            return _contextDB.User.Any(predicate);
        }

        public void Insert(Models.User model, bool saveChanges = true)
        {
            _contextDB.User.Add(model);
            if (saveChanges)
                _contextDB.SaveChanges();
        }

        public void Update(Models.User model, bool saveChanges = true)
        {
            _contextDB.ChangeTracker.Clear();
            _contextDB.User.Update(model);
            if (saveChanges)
                _contextDB.SaveChanges();
        }

        public string HashPass(string pass)
        {
            byte[] key = Encoding.ASCII.GetBytes(AUTH_TOKEN);
            HMACSHA256 myhmacsha256 = new HMACSHA256(key);
            byte[] byteArray = Encoding.ASCII.GetBytes(pass);
            MemoryStream stream = new MemoryStream(byteArray);
            string result = myhmacsha256.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);

            return result;
        }

        public string GeneratePrivateKey()
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();

            byte[] data = new byte[32];
            rng.GetBytes(data);
            return Convert.ToBase64String(data);
        }
    }
}