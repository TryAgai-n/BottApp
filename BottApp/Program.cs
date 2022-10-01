using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Enumeration;
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
                    int? _test = null;
                    Console.WriteLine($"TRY PASS: {_username} | {_id} | {_text} | {_test}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Возникло исключение! | " + ex);
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
                        if (!isSendContact)
                        {
                            await bot.SendTextMessageAsync(id, "Привет! Отправьте ваш контакт для связи", replyMarkup: Keyboards.WelcomeKeyboard);
                            return;
                        }
                        else
                        {
                            await bot.SendTextMessageAsync(id, $"Спасибо, {username}, но вы уже делились контактом. Все записано, не переживайте!", replyMarkup: Keyboards.DefaultKeyboard);
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

                    if (preparedMessage.Contains("/stats"))
                    {
                        try
                        {
                            var _text = update.Message.Text;
                            var _id = update.Message.Chat.Id;
                            var _username = update.Message.Chat.FirstName;
                            var _phonenumber = userPhone;
                            Console.WriteLine($"{_username} | {_id} | {_text} | {_phonenumber}");
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
                    await bot.SendTextMessageAsync(id, "Гружу...");
                    DownloadPhoto(bot, message);
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
                    // await bot.SendStickerAsync(id)
                    return;
                }

                if (update.Message.Type == MessageType.Sticker)
                {
                    await bot.SendTextMessageAsync(id, $"{username}, лови айдишник стика!\n {update.Message.Sticker.FileId}");
                    return;
                }

                if (update.Message.Type == MessageType.Contact && !isSendContact)
                {
                    isSendContact = true;
                    userPhone = message.Contact.PhoneNumber;
                    await bot.SendTextMessageAsync(id, $"Спасибо, {username}, ваш  номер +{userPhone} записан! ");
                    await Task.Delay(1000);
                    await bot.SendTextMessageAsync(id, "Теперь выберите необходимый пункт меню, я постарюсь вам помочь!");
                    await Task.Delay(1000);
                    await bot.SendStickerAsync(id, Stikers.stiker3.Stiker_ID, replyMarkup: Keyboards.DefaultKeyboard);
                    return;
                }
                else
                {
                    await bot.SendTextMessageAsync(id, $"Спасибо, {username}, но вы уже делились контактом. Все записано, не переживайте!");
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

        async static public void DownloadDocument(ITelegramBotClient botClient, Message message)
        {

            try
            {
                var _field = message.Document.FileId;
                var _fileInfo = await botClient.GetFileAsync(_field);
            }
            catch
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Слишком большой размер, не лезет =( Отправьте файл размером до 20МБ");
                return;
            }

            var folderName = "Document";
            var dataPath = Directory.GetCurrentDirectory() + "/DATA/";
            var newPath = Path.Combine(dataPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            var fileName = message.Document.FileName;
            var field = message.Document.FileId;
            var fileInfo = await botClient.GetFileAsync(field);
            var filePath = fileInfo.FilePath;

            string destinationFilePath = newPath + $"/{message.Chat.FirstName}__{Guid.NewGuid().ToString("N")}__{message.Chat.Id}__{fileName}";

            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();

            Console.WriteLine($"Документ {message.Document.FileName} получен от пользователя {message.Chat.FirstName} ID {message.Chat.Id}, размер документа {fileInfo.FileSize} байт. \nFULLPATH {destinationFilePath}");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных.");



            return;

        }


        async static public void DownloadPhoto(ITelegramBotClient botClient, Message message)
        {
            try
            {
                var _field = message.Photo[message.Photo.Length - 1].FileId;
                var _fileInfo = await botClient.GetFileAsync(_field);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
            }

            var folderName = "Photo";
            var dataPath = Directory.GetCurrentDirectory() + "/DATA/";
            var newPath = Path.Combine(dataPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            var fielid = message.Photo[message.Photo.Length - 1].FileId;
            var fileInfo = await botClient.GetFileAsync(fielid);
            var filePath = fileInfo.FilePath;

            string destinationFilePath = newPath + $"/{message.Chat.FirstName}__{Guid.NewGuid().ToString("N")}__{message.Chat.Id}.jpg";

            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();

            Console.WriteLine($"Документ {fielid} получен от пользователя {message.Chat.FirstName} ID {message.Chat.Id}, размер документа {fileInfo.FileSize} байт. \nFULLPATH {destinationFilePath}");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных.");

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