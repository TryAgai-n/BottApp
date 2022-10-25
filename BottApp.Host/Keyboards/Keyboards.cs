using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
//Codestyle left the chat...
namespace BottApp
{
    public class Keyboards
    {

        public static ReplyKeyboardMarkup WelcomeKeyboard = new(new[]
        {
            KeyboardButton.WithRequestContact("Поделиться номером"),
        })
        {
            ResizeKeyboard = true
        };
    }
}
