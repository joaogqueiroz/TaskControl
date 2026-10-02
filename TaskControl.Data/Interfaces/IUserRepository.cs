using TaskControl.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskControl.Data.Interfaces
{
    public interface IUserRepository
    {
        void Create(User user);
        void Update(User user);
        void Update(Guid userId, string newPassWord);
        void Delete(User user);
        List<User> Read();
        User GetById(Guid userId);
        User Get(string email);
        User Get(string email, string password);
        

    }
}
