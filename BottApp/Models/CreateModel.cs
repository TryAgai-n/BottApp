using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BottApp.Models
{
    public class CreateModel
    {
        static public void CreateUser(string userName, string userPhone, long tgId, bool isSendContact)
        {
            var ID = Guid.NewGuid().ToString();
            var model = new UserModel(userName, userPhone, tgId, ID, isSendContact);
            JsonHelper.SaveUser(model);
        }
    }
}
