using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.User;

public class UserWebRepository : AbstractRepository<UserModel>, IUserWebRepository
{
    
    public UserWebRepository(PostgresContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory) { }

    public async Task<UserModel> CreateUser(string login, string firstName, string lastName, string phone, string password)
    {
        var model = UserModel.Create(login, firstName, lastName, phone, password);
        
        var result = await CreateModelAsync(model);
        
        if(model is null)
        {
            throw new Exception("User is not created. Instantiate error");
        }
            
        return result;
    }


    public async Task<UserModel> GetOneById(int id)
    {
        var model = await DbModel.Where(x => x.Id == id).FirstAsync();

        if (model is null)
        {
            throw new Exception($"User with Id: {id} is not found");
        }

        return model;
    }
    
    public async Task<UserModel> FindOneByLogin(string login)
    {
        var model = await DbModel.Where(x => x.Login == login).FirstAsync();

        return model;
    }
    
    public async Task<UserModel> GetOneByPhone(string phone)
    {
        var model = await DbModel.Where(x => x.Phone == phone).FirstAsync();

        if (model is null)
        {
            throw new Exception($"User with phone: {phone} is not found");
        }

        return model;
    }
}