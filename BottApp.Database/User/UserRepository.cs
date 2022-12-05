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
        public async Task<UserModel> CreateUser(long uid, string? telegramFirstName, string? phone)
        {

            var model = UserModel.Create(uid, telegramFirstName, phone);


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
        
        public async Task<bool> UpdateUserFullName(UserModel model, string? firstName, string? lastName)
        {
            model.FirstName = firstName;
            model.LastName = lastName;
            
            var result = await UpdateModelAsync(model);
            return result > 0;
        }


        public async Task<bool> UpdateUserFirstName(UserModel model, string? firstName)
        {
           
            if (model.LastName == firstName)
                throw new Exception($"User on the same FirstName. FirstName: {firstName}");
            
            model.FirstName = firstName;
            var result = await UpdateModelAsync(model);
            return result > 0;
        }


        public async Task<bool> UpdateUserLastName(UserModel model, string? lastName)
        {
           
            if (model.LastName == lastName)
                throw new Exception($"User on the same LastName. LastName: {lastName}");
            
            model.LastName = lastName;
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
