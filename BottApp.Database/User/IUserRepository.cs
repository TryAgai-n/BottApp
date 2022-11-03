using System.Threading.Tasks;

namespace BottApp.Database.User
{
    public interface IUserRepository
    {
        Task<UserModel> CreateUser(long uid, string firstName, string phone);

        Task<UserModel> GetOne(int id);

        Task<UserModel?> FindOneById(int userId);
    }
}
