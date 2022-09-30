using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BottApp
{
    class Program
    {
        static public bool isSendContact = false;
        static string userPhone;
        static void Main(string[] args)
        {
            var bot = new TelegramBotClient("5601343711:AAH6cM03nZkSf0LdHr7YJLYlnk2DHdM5bqo");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
               {
                    UpdateType.Message,
                    UpdateType.EditedMessage,
               }
            };

            bot.StartReceiving(Update, Error);

            Console.ReadLine();
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
                    Console.WriteLine($"TRY PASS: {_username} | {_id} | {_text}");
                }
                catch
                {
                    Console.WriteLine("Возникло исключение!");
                    await bot.SendTextMessageAsync(message.Chat.Id, "Ой-ёй, все сломаделось!");
                    return;
                }

                var text = update.Message.Text;
                var id = update.Message.Chat.Id;
                var username = update.Message.Chat.FirstName;

                if (update.Message.Type == MessageType.Text)
                {

                    var preparedMessage = message.Text.ToLower();

                    if (preparedMessage.Contains("привет") || preparedMessage.Contains("/start"))
                    {
                        isSendContact = false;
                        await bot.SendTextMessageAsync(id, "Привет! Отправьте ваш контакт для связи", replyMarkup: Keyboards.WelcomeKeyboard);
                        return;
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

                    if (preparedMessage.Contains("/stats"))
                    {
                        try
                        {
                            var _text = update.Message.Text;
                            var _id = update.Message.Chat.Id;
                            var _username = update.Message.Chat.FirstName;
                            var _phonenumer = userPhone;
                            Console.WriteLine($"{_username} | {_id} | {_text} | {_phonenumer}");
                        }
                        catch
                        {
                            Console.WriteLine("Возникло исключение!");
                            return;
                        }

                        await bot.SendTextMessageAsync(id, $"Статистика: \nbool - isSendContact: {isSendContact}, Phone {userPhone ?? "null"}");
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
                    LogViewer(message);
                    await bot.SendTextMessageAsync(id, "Хорошее фото, но лучше будет, если выслать документом =)");
                    return;
                }

                if (update.Message.Type == MessageType.Document)
                {
                    DownloadDocument(bot, message);
                    return;
                }

                if (update.Message.Type == MessageType.Voice)
                {
                    await bot.SendTextMessageAsync(id, $"{username}, я еще не умею обрабатывать такие команды, но уже учусь!");
                    return;
                }

                if (update.Message.Type == MessageType.Contact && !isSendContact)
                {
                    isSendContact = true;
                    userPhone = message.Contact.PhoneNumber;
                    await bot.SendTextMessageAsync(id, $"Спасибо, {username}, ваш  номер +{userPhone} записан! Теперь выберите необходимый пункт меню, я постарюсь вам помочь!", replyMarkup: Keyboards.DefaultKeyboard);
                    return;
                }
                else
                {
                    await bot.SendTextMessageAsync(id, $"Спасибо, {username}, но вы уже делились контактом. Все записано, не переживайте!");
                    return;
                }



            }
        }


        async static Task Error(ITelegramBotClient client, Exception exception, CancellationToken clt)
        {
            Console.WriteLine("Fatall error | " + exception);
            return;
        }

        async static public void DownloadDocument(ITelegramBotClient botClient, Message message)
        {
            var field = message.Document.FileId;
            try
            {
                var _fileInfo = await botClient.GetFileAsync(field);
            }
            catch
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Слишком большой размер, не лезет =( Отправьте файл размером до 20МБ");
                return;
            }

            var fileInfo = await botClient.GetFileAsync(field);
            var filePath = fileInfo.FilePath;
            Console.WriteLine($"Документ {message.Document.FileName} получен от пользователя {message.Chat.FirstName} ID {message.Chat.Id}, размер документа { fileInfo.FileSize} байт.");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных.");
            //Enter the path for files
            string destinationFilePath = $@"C:\Users\LoveYouAgain\Desktop\TelegramBD\{message.Document.FileName}";
            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();
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