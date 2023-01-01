using System;
using System.Threading.Tasks;
using BottApp.Database.Document;

namespace BottApp.Database.User
{
    public interface IUserRepository
    {
        
        Task<UserModel> CreateUser(TelegramProfile telegramProfile);

        Task<UserModel> GetOneByUid(long uid);

        Task<UserModel?> FindOneByUid(long userId);

        Task<bool> UpdateUserPhone(UserModel model, string phone);

        Task<bool> UpdateUserFullName(UserModel model, Profile profile);
        Task<bool> ChangeOnState(UserModel model, OnState onState);

        Task<bool> ChangeViewMessageId(UserModel model, int messageId);

        #region TestList

        // Task<bool> ChangeUserNomination(UserModel model, DocumentNomination nomination);

        #endregion

    }
}
