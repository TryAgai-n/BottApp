using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BottApp.Database.User
{
    public class UserRepository : AbstractRepository<UserModel>, IUserRepository
    {
        public UserRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {

        }


        public async Task<UserModel> CreateUser(long uid, string? firstName, string? phone)
        {

            var model = UserModel.Create(uid, firstName, phone);


            var result = await CreateModelAsync(model);


            if(model == null)
            {
                throw new Exception("User is not created. Db error");
            }


            return result;
        }

        public async Task<UserModel> GetOne(int id)
        {
            var model = await FindOne(id);
            if(model == null)
            {
                throw new Exception($"User model by id: {id} is not found");
            }
            return model;
        }

        public async Task<UserModel?> FindOneById(int userId)
        {
            return await DbModel.FirstOrDefaultAsync(x => x.UId == userId);
        }
        

        public async Task<bool> UpdateUserPhone(UserModel model, string phone)
        {
            model.Phone = phone; 
            var result = await UpdateModelAsync(model);

            return result > 0;
        }

    }
}
