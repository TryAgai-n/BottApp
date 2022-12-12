using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BottApp.Database.Document;
using Microsoft.EntityFrameworkCore;

namespace BottApp.Database.User
{
    public class UserRepository : AbstractRepository<UserModel>, IUserRepository
    {
        public UserRepository(PostgreSqlContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
        {

        }
        public async Task<UserModel> CreateUser(TelegramProfile telegramProfile)
        {

            var model = UserModel.Create(telegramProfile);


            var result = await CreateModelAsync(model);


            if(model == null)
            {
                throw new Exception("User is not created. Db error");
            }


            return result;
        }


        public async Task<UserModel> GetOneByUid(long uid)
        {
            var model = await DbModel.Where(x => x.UId == uid).FirstAsync();
            if(model == null)
            {
                throw new Exception($"User with Uid: {uid} is not found");
            }
            return model;
        }


        public async Task<UserModel?> FindOneByUid(long userId)
        {
            return await DbModel.FirstOrDefaultAsync(x => x.UId == userId);
        }
        

        public async Task<bool> UpdateUserPhone(UserModel model, string phone)
        {
            model.Phone = phone; 
            var result = await UpdateModelAsync(model);

            return result > 0;
        }
        
        public async Task<bool> UpdateUserFullName(UserModel model, Profile profile)
        {

            model.SetUserProfile(profile);
            var result = await UpdateModelAsync(model);
            
            return result > 0;
        }
        
        public async Task<bool> ChangeOnState(UserModel model, OnState onState)
        {
            if (model.OnState == onState)
            {
                throw new Exception($"User on the same state. State: {onState}");
            }
            
            model.OnState = onState;
            var result = await UpdateModelAsync(model);

            return result > 0;
        }


        // public async Task<bool> ChangeUserNomination(UserModel model, DocumentNomination nomination)
        // {
        //     var user = await GetOneByUid(model.UId);
        //
        //     return true;
        // }


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
