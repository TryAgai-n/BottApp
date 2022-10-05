using BottApp.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BottApp
{
    public class Program
    {

        static public bool isSendContact = false;

        public static void initReceiver(TelegramBotClient bot)
        {
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

        async static Task Update(ITelegramBotClient bot, Update update, CancellationToken CLToken)
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
                        if (!isSendContact)
                        {
                            await bot.SendTextMessageAsync(id, "Привет! Отправьте ваш контакт для связи", replyMarkup: Keyboards.WelcomeKeyboard);
                            return;
                        }
                        else
                        {
                            await bot.SendTextMessageAsync(id, $"Спасибо, {firstName}, но вы уже делились контактом. Все записано, не переживайте!", replyMarkup: Keyboards.DefaultKeyboard);
                            return;
                        }

                    }


                    if (preparedMessage.Contains("контакты"))
                    {
                        await bot.SendTextMessageAsync(id, "Держи!", replyMarkup: Keyboards.inlineUrlKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("голосование"))
                    {
                        await bot.SendTextMessageAsync(id, "Выбирай", replyMarkup: Keyboards.VotesKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("главное меню"))
                    {
                        await bot.SendTextMessageAsync(id, "Выбирай", replyMarkup: Keyboards.DefaultKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("отправить документ"))
                    {
                        await bot.SendTextMessageAsync(id, "Выберите документы через скрепку и отправьте в чат. Размер одного документа должен быть до 20 Мб.", replyMarkup: Keyboards.DefaultKeyboard);
                        return;
                    }
                    if (preparedMessage.Contains("отладка"))
                    {
                        await bot.SendTextMessageAsync(id, "Раздел отладки", replyMarkup: Keyboards.debuggingKeyboard);
                        return;
                    }
                    if (preparedMessage.Contains("отправить контакт"))
                    {
                        await bot.SendTextMessageAsync(id, "Жду!", replyMarkup: Keyboards.WelcomeKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("/stats"))
                    {
                        try
                        {
                            var _text = update.Message.Text;
                            var _id = update.Message.Chat.Id;
                            var _username = update.Message.Chat.FirstName;
                           // var _phonenumber = userPhone;
                            Console.WriteLine($"{_username} | {_id} | {_text} | {"null"/*_phonenumber*/}");
                        }
                        catch
                        {
                            Console.WriteLine("Возникло исключение!");
                            return;
                        }

                        await bot.SendTextMessageAsync(id, $"Статистика: \nbool - isSendContact: {isSendContact}, Phone {/*userPhone ?? */"null"}");
                        return;
                    }

                    else
                    {
                        await bot.SendTextMessageAsync(id, "Не совсем понял вас. (возможно раздел еще в разработке)", replyMarkup: Keyboards.DefaultKeyboard);
                        return;
                    }


                }

                if (update.Message.Type == MessageType.Photo)
                {
                    await bot.SendTextMessageAsync(id, "Гружу...");
                    DownloaManager.DownloadDocument(bot, message, update.Message.Type);
                    return;
                }

                if (update.Message.Type == MessageType.Document)
                {
                    DownloaManager.DownloadDocument(bot, message, update.Message.Type);
                    return;
                }

                if (update.Message.Type == MessageType.Voice)
                {
                    DownloaManager.DownloadDocument(bot, message, update.Message.Type);
                    await bot.SendStickerAsync(id, Stikers.stiker1.Stiker_ID);
                    await Task.Delay(500);
                    await bot.SendTextMessageAsync(id, $"{firstName}, я еще не умею обрабатывать такие команды, но уже учусь!");
                    return;
                }

                if (update.Message.Type == MessageType.Sticker)
                {
                    await bot.SendTextMessageAsync(id, $"{firstName}, лови айдишник стика!\n {update.Message.Sticker.FileId}");
                    return;
                }

                if (update.Message.Type == MessageType.Contact) //&& !isSendContact
                {
                    var userPhone = message.Contact.PhoneNumber;

                    var model = new UserModel(userName, userPhone, id, true);
                    JsonHelper.SaveUser(model);

                    isSendContact = true;
                    
                    await bot.SendTextMessageAsync(id, $"Спасибо, {firstName}, ваш  номер +{userPhone} записан! ");
                    await Task.Delay(1000);
                    await bot.SendTextMessageAsync(id, "Теперь выберите необходимый пункт меню, я постарюсь вам помочь!");
                    await Task.Delay(1000);
                    await bot.SendStickerAsync(id, Stikers.stiker3.Stiker_ID, replyMarkup: Keyboards.DefaultKeyboard);
                    return;
                }
                else
                {
                    await bot.SendTextMessageAsync(id, $"Спасибо, {firstName}, но вы уже делились контактом. Все записано, не переживайте!");
                    await bot.SendStickerAsync(id, Stikers.stiker2.Stiker_ID, replyMarkup: Keyboards.DefaultKeyboard);
                    return;
                }



            }
        }


        async static Task Error(ITelegramBotClient client, Exception exception, CancellationToken clt)
        {
            Console.WriteLine("Fatall error | " + exception);
            return;
        }

        static public void LogViewer(Message message)
        {
            if (message.Document != null)
            {
                Console.WriteLine($"{message.Chat.FirstName ?? "undefined"}   |  {message.Type}  |  {message.Document.FileName} ");
            }
            if (message.Text != null)
            {
                Console.WriteLine($"{message.Chat.FirstName ?? "undefined"}   |  {message.Type}  |  Message: {message.Text ?? "Null"} ");
            }
            if (message.Photo != null)
            {
                Console.WriteLine($"{message.Chat.FirstName ?? "undefined"}   |  {message.Type}  |  Message: {message.Photo} ");
            }
        }
    }
}