using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.User.UserFlag;

public class UserFlagRepository : AbstractRepository<UserFlagModel>, IUserFlagRepository
{
    public UserFlagRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {

    }
    
    public async Task<UserFlagModel> CreateModel()
    {
        var model = UserFlagModel.CreateModel();

        var result = await CreateModelAsync(model);
        if (result == null)
        {
            throw new Exception("User Flag model is not created");
        }

        return result;
    }
}