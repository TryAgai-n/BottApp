using BottApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace BottApp
{
    public class JsonHelper
    {
        public async static Task SaveUser(UserModel model)
        {
            using (FileStream fs = new FileStream("Users.json", FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync<UserModel>(fs, model);
                Console.WriteLine("Data has been saved to file");
                return;
            }
        }

        public async static Task LoadUserInformation(UserModel model)
        {
            //:TODO Вывод информации о записях в таблице
        }
        public async static Task CheckData()
        {
            //:TODO Проверка, если запись о пользователе в JSON файле
        }

    }
}
