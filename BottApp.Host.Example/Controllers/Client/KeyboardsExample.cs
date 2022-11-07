using Telegram.Bot.Types.ReplyMarkups;
namespace BottApp.Host.Example.Controllers.Client
{
    public class KeyboardsExample
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
            KeyboardButton.WithRequestContact("Поделиться номером"),
        })
        {
            ResizeKeyboard = true
        };

        public static InlineKeyboardMarkup inlineUrlKeyboard = new(new[]
       {
        InlineKeyboardButton.WithUrl(
            text: "Сслыка на корпоративный сайт",
            url: "http://imp.dom"
        )
        });
    }
}
