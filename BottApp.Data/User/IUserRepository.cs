using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BottApp.Data.User
{
    public interface IUserRepository
    {
        Task<UserModel> CreateUser(string firstName, string phone, bool isSendContact);

        Task<UserModel> GetOne(int id);
    }
            
}
