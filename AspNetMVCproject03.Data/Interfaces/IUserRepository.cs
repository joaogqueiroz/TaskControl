using AspNetMVCproject03.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Data.Interfaces
{
    public interface IUserRepository
    {
        void Create(User user);
        void Update(User user);
        void Delete(User user);
        List<User> Read();
        User GetById(Guid id);
        User Get(string email);
        User Get(string email, string password);
    }
}
