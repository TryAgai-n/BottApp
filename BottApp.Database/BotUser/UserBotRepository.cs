using System;
using System.Linq;
using System.Threading.Tasks;
using BottApp.Database.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database.BotUser
{
    public class UserBotRepository : AbstractRepository<UserBotModel>, IUserBotRepository
    {
        public UserBotRepository(PostgresContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {

        }
        public async Task<UserBotModel> CreateUser(long uid, string? firstName, string? phone)
        {

            var model = UserBotModel.Create(uid, firstName, phone);


            var result = await CreateModelAsync(model);


            if(model == null)
            {
                throw new Exception("User is not created. Db error");
            }
            
            return result;
        }

        public async Task<UserBotModel> GetOneById(int id)
        {
            var model = await DbModel.Where(x => x.Id == id).FirstAsync();
            if(model == null)
            {
                throw new Exception($"User with Id: {id} is not found");
            }
            return model;
        }
        

        public async Task<UserBotModel> GetOneByUid(long uid)
        {
            var model = await DbModel.Where(x => x.UId == uid).FirstAsync();
            if(model == null)
            {
                throw new Exception($"User with Uid: {uid} is not found");
            }
            return model;
        }


        public async Task<UserBotModel?> FindOneByUid(long userId)
        {
            return await DbModel.FirstOrDefaultAsync(x => x.UId == userId);
        }
        

        public async Task<bool> UpdateUserPhone(UserBotModel model, string phone)
        {
            model.Phone = phone; 
            var result = await UpdateModelAsync(model);

            return result > 0;
        }

        public async Task<bool> ChangeOnState(UserBotModel model, OnState onState)
        {
            if (model.OnState == onState)
            {
                throw new Exception($"User on the same state. State: {onState}");
            }
            
            model.OnState = onState;
            var result = await UpdateModelAsync(model);

            return result > 0;
        }
        
        public async Task<bool> ChangeOnStateByUID(long uid, OnState onState)
        {
            var user = await GetOneByUid(uid);
            
            if (user.OnState == onState) 
                throw new Exception($"User on the same state. State: {onState}");
            
            user.OnState = onState;
            var result = await UpdateModelAsync(user);

            return result > 0;
        }
    }
}
