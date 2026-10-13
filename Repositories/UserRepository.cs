using EComWeb.Models;
using Microsoft.EntityFrameworkCore;
namespace EComWeb.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MyContext _context;

        public UserRepository(MyContext context)
        {
            _context = context;
        }

        public Customer GetByEmail(string email)
        {
            return _context.tbl_customer.FirstOrDefault(c => c.customer_email == email);
        }

        public void Add(Customer customer)
        {
            _context.tbl_customer.Add(customer);
        }

        public bool EmailExists(string email)
        {
            return _context.tbl_customer.Any(c => c.customer_email == email);
        }

        public bool SaveChanges()
        {
            try
            {
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
