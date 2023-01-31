using System;
using System.Threading.Tasks;
using BottApp.Database.Document;

namespace BottApp.Database.User
{
    public interface IUserRepository
    {
        
        Task<UserModel> CreateUser(TelegramProfile telegramProfile);

        Task<UserModel> GetOneByUid(long uid);
        Task<UserModel?> FindOneByUid(long uid);
        Task<UserModel?> FindOneByUidAsNoTracking(long uid);
        Task<UserModel?> FindOneById(int id);
        

        Task<bool> UpdateUserPhone(UserModel model, string phone);

        Task<bool> UpdateUserFullName(UserModel model, Profile profile);
        Task<bool> ChangeOnState(UserModel model, OnState onState);

        Task<bool> ChangeViewMessageId(UserModel model, int messageId);
        
        Task<bool> ChangeViewDocumentId(UserModel model, int documentId);

        Task<bool> UpdateUser(UserModel user);

    }
}
