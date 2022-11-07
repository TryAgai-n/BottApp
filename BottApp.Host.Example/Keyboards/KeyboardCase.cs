using Telegram.Bot.Types.ReplyMarkups;

//Codestyle left the chat...
namespace BottApp.Host.Example.Keyboards
{
    public class KeyboardCase
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
