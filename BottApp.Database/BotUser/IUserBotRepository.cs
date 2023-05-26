using System.Threading.Tasks;

namespace BottApp.Database.User
{
    public interface IUserBotRepository
    {
        Task<UserBotModel> CreateUser(long uid, string firstName, string? phone);

        Task<UserBotModel> GetOneByUid(long uid);

        Task<UserBotModel> GetOneById(int id);

        Task<UserBotModel?> FindOneByUid(long userId);

        Task<bool> UpdateUserPhone(UserBotModel model, string phone);

        Task<bool> ChangeOnState(UserBotModel model, OnState onState);
        
    }
}
