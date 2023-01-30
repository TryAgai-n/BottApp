using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        
        public async Task<UserModel?> FindOneByUid(long uid)
        {
            return await DbModel.FirstOrDefaultAsync(x => x.UId == uid);
        }
        
        public async Task<UserModel?> FindOneByUidAsNoTracking(long uid)
        {
            return await DbModel.AsNoTracking().FirstOrDefaultAsync(x => x.UId == uid);
        }

        
        public async Task<UserModel?> FindOneById(int id)
        {
            return await DbModel.FirstOrDefaultAsync(x => x.Id == id);
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


        public async Task<bool> ChangeViewMessageId(UserModel model, int messageId)
        {
            model.ViewMessageId = messageId;
            var result = await UpdateModelAsync(model);
            return result > 0;
        }
        
        public async Task<bool> ChangeViewDocumentId(UserModel model, int documentId)
        {
            model.ViewDocumentId = documentId;
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


        public async Task<List<UserModel>> FindUserByFirstName(string firstName)
        {
            return await DbModel.Where(x => x.FirstName == firstName).ToListAsync();
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
