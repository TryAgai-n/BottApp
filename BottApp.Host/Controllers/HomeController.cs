using BottApp.Data;
using BottApp.Data.Book;
using BottApp.Data.User;
using BottApp.Host.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BottApp.Host.Controllers
{
    public class HomeController : Controller
    {

        private readonly IDatabaseContainer _databaseContainer;

        public  HomeController(IDatabaseContainer databaseContainer)
        {
            _databaseContainer = databaseContainer;
        }

        // [HttpGet]
        // public async Task<UserModel> Test(string firstName, string userPhone, bool isSendContact)
        // {
        // var user = await _databaseContainer.User.CreateUser(firstName,userPhone, isSendContact);
        // return user;
        // }

        public async Task<BookModel> Test2()
        {
            var book = await _databaseContainer.Book.CreateBook("Book", "тест");
            return book;
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