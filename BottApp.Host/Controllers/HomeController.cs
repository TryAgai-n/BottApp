using BottApp.Database;
using BottApp.Database.User;
using BottApp.Host.Controllers.Client;
using Microsoft.AspNetCore.Mvc;

namespace BottApp.Host.Controllers
{
    public class HomeController : Controller
    {

        private readonly IDatabaseContainer _databaseContainer;

        public  HomeController(IDatabaseContainer databaseContainer)
        {
            _databaseContainer = databaseContainer;
        }

        [HttpGet]
        public async Task<UserModel> Test(int uid, string firstName, string userPhone, bool isSendContact)
        {
        var user = await _databaseContainer.User.CreateUser(uid, firstName,userPhone, isSendContact);
        return user;
        }

        public void Createuser(int uid, string firstName, string userPhone, bool isSendContact)
        {
            Test(uid, firstName, userPhone, isSendContact);
            return;
        }
        


        [HttpGet]
        public async Task<UserModel> GetOneById(int id)
        {
           return await _databaseContainer.User.GetOne(id);

        }


        // public async void AddUserOnDb(int uid, string firstName, string userPhone, bool isSendContact)
        // {
        //     await _databaseContainer.User.CreateUser(uid, firstName, userPhone, isSendContact);
        //     return;
        // }

    }
}