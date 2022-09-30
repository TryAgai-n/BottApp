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
        public static ReplyKeyboardMarkup DefaultKeyboard =
            new(new[]
            {

                new KeyboardButton[] { "Контакты", "Обратиться в службу поддержки" },
                new KeyboardButton[] { "Голосование", "Отправить документ" },
                new KeyboardButton[] { "Отладка" }
            })
            {
                ResizeKeyboard = true
            };


        public static ReplyKeyboardMarkup debuggingKeyboard =
          new(new[]
          {
                new KeyboardButton[] { "/stats", "Главное меню" },
          })
          {
              ResizeKeyboard = true
          };

        public static ReplyKeyboardMarkup VotesKeyboard =
            new(new[]
            {

                new KeyboardButton[] { "Айтишник тысячалетия", "Вам повестка" },
                new KeyboardButton[] { "Главное меню" }
            })
            {
                ResizeKeyboard = true
            };


        public static ReplyKeyboardMarkup WelcomeKeyboard = new(new[]
        {
            KeyboardButton.WithRequestContact("Share Contact"),
        });

        public static InlineKeyboardMarkup inlineUrlKeyboard = new(new[]
       {
        InlineKeyboardButton.WithUrl(
            text: "Сслыка на корпоративный сайт",
            url: "http://imp.dom"
        )
        });
    }
}
