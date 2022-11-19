using System;
using System.Threading.Tasks;

namespace BottApp.Database.User
{
    public interface IUserRepository
    {
        Task<UserModel> CreateUser(long uid, string firstName, string? phone);

        Task<UserModel> GetOneByUid(long uid);

        Task<UserModel?> FindOneByUid(long userId);

        Task<bool> UpdateUserPhone(UserModel model, string phone);

        Task<bool> ChangeOnState(UserModel model, OnState onState);
        
    }
}
