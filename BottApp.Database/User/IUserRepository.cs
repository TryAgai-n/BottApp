using System;
using System.Threading.Tasks;

namespace BottApp.Database.User
{
    public interface IUserRepository
    {
        Task<UserModel> CreateUser(long uid, string firstName, string? phone, string state);

        Task<UserModel> GetOneByUid(long uid);

        Task<UserModel?> FindOneByUid(int userId);

        Task<bool> UpdateUserPhone(UserModel model, string phone);

        Task<bool> SetState(UserModel model, string state);
        
        Task<string> GetStateByUid(long uid);
        
    }
}
