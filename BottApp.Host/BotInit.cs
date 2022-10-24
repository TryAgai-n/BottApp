using BottApp.Data.User;
using BottApp.Data;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BottApp.Host.Controllers;

namespace BottApp.Host
{
    public class BotInit
    {

        public void initReceiver(string token)
        {

            var bot = new TelegramBotClient(token);
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message,
                    UpdateType.EditedMessage,
                }
            };

            bot.StartReceiving(Update, Error);
        }

        async Task Update(ITelegramBotClient bot, Update update, CancellationToken CLToken)
        {


            var message = update.Message;

            if (update.Type == UpdateType.Message)
            {
                try
                {
                    var _text = update.Message.Text;
                    var _id = update.Message.Chat.Id;
                    var _username = update.Message.Chat.FirstName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Возникло исключение! | " + ex);
                    await bot.SendTextMessageAsync(message.Chat.Id, "Ой-ёй, все сломаделось!");
                    return;
                }

                var id = update.Message.Chat.Id;
                var firstName = update.Message.Chat.FirstName;
                var userName = update.Message.Chat.Username;

                if (update.Message.Type == MessageType.Text)
                {

                    var preparedMessage = message.Text.ToLower();

                    if (preparedMessage.Contains("привет") || preparedMessage.Contains("/start"))
                    {
                        await bot.SendTextMessageAsync(id, "Привет! Отправьте ваш контакт для связи", replyMarkup: Keyboards.WelcomeKeyboard);
                        return;
                    }

                    else
                    {
                        await bot.SendTextMessageAsync(id, "Не совсем понял вас. (возможно раздел еще в разработке)", replyMarkup: Keyboards.WelcomeKeyboard);
                        return;
                    }
                }

                if (update.Message.Type == MessageType.Contact)
                {
                    Console.WriteLine("Contact is Send");

                    var userPhone = message.Contact.PhoneNumber;
                    ///////
                    //TO DO : Здесь должна быть логика записи полученной инфы в базу
                    //Example: AddUserModelToDB(firstName, userPhone, true);
                    ///////

                    await bot.SendTextMessageAsync(id, $"Спасибо, {firstName}, ваш  номер +{userPhone} записан! ");
                    await Task.Delay(1000);
                    await bot.SendTextMessageAsync(id, "Теперь выберите необходимый пункт меню, я постарюсь вам помочь!");

                    return;
                }
            }
        }

        async Task Error(ITelegramBotClient client, Exception exception, CancellationToken CLToken)
        {
            Console.WriteLine("Fatal error | " + exception);
            return;
        }


     
    }
}
