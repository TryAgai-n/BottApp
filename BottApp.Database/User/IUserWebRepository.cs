using System.Threading.Tasks;

namespace BottApp.Database.User;

public interface IUserWebRepository
{
    Task<UserModel> CreateUser(string login, string firstName, string lastName, string phone, string password);

    Task<UserModel?> FindOneByLogin(string login);

    Task<UserModel> GetOneByPhone(string phone);

    Task<UserModel> GetOneById(int id);
}