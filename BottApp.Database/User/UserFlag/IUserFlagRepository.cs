using System.Threading.Tasks;

namespace BottApp.Database.User.UserFlag;

public interface IUserFlagRepository
{
    Task<UserFlagModel> CreateModel();
}