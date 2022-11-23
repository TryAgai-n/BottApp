using System;
using System.Threading.Tasks;

namespace BottApp.Database.User
{
    public interface IUserRepository
    {
        Task<UserModel> CreateUser(long uid, string telegramFirstName, string? phone);

        Task<UserModel> GetOneByUid(long uid);

        Task<UserModel?> FindOneByUid(long userId);

        Task<bool> UpdateUserPhone(UserModel model, string phone);

        Task<bool> UpdateUserFullName(UserModel model, string? firstName, string? lastName);
        
        Task<bool> UpdateUserFirstName(UserModel model, string? firstName);
        Task<bool> UpdateUserLastName(UserModel model, string? lastName);
        
        Task<bool> ChangeOnState(UserModel model, OnState onState);
        
    }
}
