using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BottApp.Data.User
{
    public class UserRepository : AbstractRepository<UserModel>, IUserRepository
    {
        public UserRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {

        }


        public async Task<UserModel> CreateUser(string firstName, string phone, bool isSendContact)
        {

            var model = UserModel.Create(firstName, phone, isSendContact);


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
                throw new Exception("User model is not found");
            }
            return model;
        }
    }
}
