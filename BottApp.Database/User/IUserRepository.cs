using System.Threading.Tasks;

namespace BottApp.Database.User
{
    public interface IUserRepository
    {
        Task<UserModel> CreateUser(int uid, string firstName, string phone, bool isSendContact);

        Task<UserModel> GetOne(int id);
    }
}
