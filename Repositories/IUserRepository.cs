using EComWeb.Models;

namespace EComWeb.Repositories
{
    public interface IUserRepository
    {
        Customer GetByEmail(string email);
        void Add(Customer customer);
        bool EmailExists(string email);
        bool SaveChanges();
    }
}
