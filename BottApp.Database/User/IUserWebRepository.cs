using System.Threading.Tasks;
using BottApp.Database.User;

namespace BottApp.Database.WebUser;

public interface IUserWebRepository
{
    Task<UserModel> CreateUser(string firstName, string lastName, string password, string phone);
    Task<UserModel> GetOneById(int id);
}